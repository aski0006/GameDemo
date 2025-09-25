using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using AsakiFramework;

/// <summary>
/// Editor window to inspect ActionSystem internals and visualize call stack / subscriptions.
/// - Uses reflection to read private fields: _preSubs, _postSubs, _perfSubs, _callStack, _runningCount.
/// - Captures "[ActionSystem] 死循环反应" errors from Editor log to display dead-loop events with timestamp and stacktrace.
/// - Provides quick actions: Refresh, ClearAll(), Force sample PerformGameAction for parameterless GameAction types.
/// 
/// Place in an Editor folder. Works in the Editor only.
/// </summary>
public class ActionSystemDebuggerWindow : EditorWindow
{
    private Vector2 leftScroll;
    private Vector2 rightScroll;
    private Vector2 bottomScroll;

    private bool autoRefresh = true;
    private double lastRefreshTime;
    private const double RefreshInterval = 0.35;

    // Reflection caches
    private FieldInfo fi_preSubs;
    private FieldInfo fi_postSubs;
    private FieldInfo fi_perfSubs;
    private FieldInfo fi_callStack;
    private FieldInfo fi_runningCount;
    private PropertyInfo pi_isRunning;

    // Cached snapshots
    private Dictionary<Type, List<Delegate>> preSnapshot = new();
    private Dictionary<Type, List<Delegate>> postSnapshot = new();
    private Dictionary<Type, Delegate> perfSnapshot = new();
    private List<Type> callStackSnapshot = new();
    private int runningCountSnapshot = 0;
    private bool isRunningSnapshot = false;

    // Dead-loop events captured from Editor log
    private readonly List<DeadLoopEvent> deadLoopEvents = new List<DeadLoopEvent>();

    // For test Perform
    private Type[] actionTypes;
    private string[] actionTypeNames;
    private int selectedActionTypeIndex = 0;

    // UI tree view for subscribers (simple)
    private bool showPre = true;
    private bool showPost = true;
    private bool showPerf = true;
    private bool showCallStack = true;
    private bool showDeadLoopLog = true;

    [MenuItem("Asaki 框架/动作系统 Debugger")]
    public static void ShowWindow()
    {
        var w = GetWindow<ActionSystemDebuggerWindow>("ActionSystem Debugger");
        w.minSize = new Vector2(700, 300);
        w.RefreshReflectionFields();
        w.RefreshActionTypeList();
    }

    private void OnEnable()
    {
        RefreshReflectionFields();
        RefreshNow();
        Application.logMessageReceived += OnLogMessageReceived;
        EditorApplication.update += OnEditorUpdate;
        RefreshActionTypeList();
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (!autoRefresh) return;
        if (EditorApplication.timeSinceStartup - lastRefreshTime > RefreshInterval)
        {
            RefreshNow();
        }
    }

    private void RefreshReflectionFields()
    {
        var asm = typeof(ActionSystem).Assembly;
        var t = typeof(ActionSystem);
        fi_preSubs = t.GetField("_preSubs", BindingFlags.NonPublic | BindingFlags.Instance);
        fi_postSubs = t.GetField("_postSubs", BindingFlags.NonPublic | BindingFlags.Instance);
        fi_perfSubs = t.GetField("_perfSubs", BindingFlags.NonPublic | BindingFlags.Instance);
        fi_callStack = t.GetField("_callStack", BindingFlags.NonPublic | BindingFlags.Instance);
        fi_runningCount = t.GetField("_runningCount", BindingFlags.NonPublic | BindingFlags.Instance);
        pi_isRunning = t.GetProperty("IsRunning", BindingFlags.Public | BindingFlags.Instance);
    }

    private void RefreshNow()
    {
        lastRefreshTime = EditorApplication.timeSinceStartup;

        try
        {
            var instance = ActionSystem.Instance;
            if (instance == null)
                return;

            // pre subs
            preSnapshot.Clear();
            postSnapshot.Clear();
            perfSnapshot.Clear();
            callStackSnapshot.Clear();

            if (fi_preSubs != null)
            {
                var raw = fi_preSubs.GetValue(instance) as IDictionary;
                if (raw != null)
                {
                    foreach (DictionaryEntry de in raw)
                    {
                        var key = de.Key as Type;
                        var list = de.Value as IList<Action<GameAction>>;
                        if (key != null && list != null)
                        {
                            preSnapshot[key] = list.Cast<Delegate>().ToList();
                        }
                    }
                }
            }

            if (fi_postSubs != null)
            {
                var raw = fi_postSubs.GetValue(instance) as IDictionary;
                if (raw != null)
                {
                    foreach (DictionaryEntry de in raw)
                    {
                        var key = de.Key as Type;
                        var list = de.Value as IList<Action<GameAction>>;
                        if (key != null && list != null)
                        {
                            postSnapshot[key] = list.Cast<Delegate>().ToList();
                        }
                    }
                }
            }

            if (fi_perfSubs != null)
            {
                var raw = fi_perfSubs.GetValue(instance) as IDictionary;
                if (raw != null)
                {
                    foreach (DictionaryEntry de in raw)
                    {
                        var key = de.Key as Type;
                        var func = de.Value as Delegate;
                        if (key != null && func != null)
                        {
                            perfSnapshot[key] = func;
                        }
                    }
                }
            }

            if (fi_callStack != null)
            {
                var raw = fi_callStack.GetValue(instance) as ICollection;
                if (raw != null)
                {
                    foreach (var v in raw)
                    {
                        if (v is Type tt) callStackSnapshot.Add(tt);
                    }
                }
            }

            if (fi_runningCount != null)
            {
                var val = fi_runningCount.GetValue(instance);
                if (val is int i) runningCountSnapshot = i;
            }
            if (pi_isRunning != null)
            {
                var val = pi_isRunning.GetValue(instance);
                if (val is bool b) isRunningSnapshot = b;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ActionSystemDebugger.RefreshNow failed: {ex}");
        }

        Repaint();
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            if (!string.IsNullOrEmpty(condition) && condition.Contains("[ActionSystem] 死循环反应"))
            {
                // try to extract type name from message e.g. "[ActionSystem] 死循环反应：InjuryWithoutSourceGA"
                string extracted = "";
                var idx = condition.IndexOf("：");
                if (idx >= 0 && idx < condition.Length - 1)
                    extracted = condition.Substring(idx + 1).Trim();
                deadLoopEvents.Insert(0, new DeadLoopEvent
                {
                    Time = DateTime.Now,
                    Message = condition,
                    TypeName = extracted,
                    StackTrace = stackTrace
                });

                // keep last 50
                if (deadLoopEvents.Count > 50) deadLoopEvents.RemoveRange(50, deadLoopEvents.Count - 50);
                Repaint();
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        DrawToolbar();

        EditorGUILayout.BeginHorizontal();

        DrawLeftPanel();   // subscribers
        DrawRightPanel();  // call stack & deadloop log

        EditorGUILayout.EndHorizontal();

        DrawBottomPanel(); // test / utilities

        EditorGUILayout.EndVertical();

        if (Event.current.type == EventType.Repaint && !autoRefresh)
        {
            // nothing
        }
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) RefreshNow();
        autoRefresh = GUILayout.Toggle(autoRefresh, "Auto", EditorStyles.toolbarButton);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ClearAll()", EditorStyles.toolbarButton))
        {
            if (EditorUtility.DisplayDialog("Clear ActionSystem", "Call ActionSystem.ClearAll() ?", "Yes", "No"))
            {
                try
                {
                    ActionSystem.Instance?.ClearAll();
                    RefreshNow();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed ClearAll: {ex}");
                }
            }
        }

        if (GUILayout.Button("Refresh Types (editor)", EditorStyles.toolbarButton))
        {
            RefreshActionTypeList();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.56f));
        EditorGUILayout.LabelField("Subscriptions", EditorStyles.boldLabel);

        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

        EditorGUILayout.LabelField($"IsRunning: {isRunningSnapshot}    RunningCount: {runningCountSnapshot}", EditorStyles.helpBox);

        // Pre
        showPre = EditorGUILayout.Foldout(showPre, $"Pre Subscriptions ({preSnapshot.Count})");
        if (showPre)
        {
            foreach (var kv in preSnapshot.OrderBy(k => k.Key.FullName))
            {
                DrawTypeSubscriptionRow(kv.Key, kv.Value, ActionSystem.ReactionTiming.Pre);
            }
        }

        // Post
        showPost = EditorGUILayout.Foldout(showPost, $"Post Subscriptions ({postSnapshot.Count})");
        if (showPost)
        {
            foreach (var kv in postSnapshot.OrderBy(k => k.Key.FullName))
            {
                DrawTypeSubscriptionRow(kv.Key, kv.Value, ActionSystem.ReactionTiming.Post);
            }
        }

        // Perform
        showPerf = EditorGUILayout.Foldout(showPerf, $"Performers ({perfSnapshot.Count})");
        if (showPerf)
        {
            foreach (var kv in perfSnapshot.OrderBy(k => k.Key.FullName))
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(kv.Key.FullName, GUILayout.MaxWidth(320));
                var method = kv.Value?.Method;
                var target = kv.Value?.Target;
                EditorGUILayout.LabelField(method != null ? $"{method.DeclaringType?.Name}.{method.Name}" : "<none>");
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawTypeSubscriptionRow(Type t, List<Delegate> delegates, ActionSystem.ReactionTiming timing)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(t.FullName, GUILayout.MaxWidth(360));
        EditorGUILayout.LabelField($"Subscribers: {delegates.Count}", GUILayout.Width(100));
        if (GUILayout.Button("Show", GUILayout.Width(48)))
        {
            // expand a little by opening a small inspector window - here we toggle a foldout stored in EditorPrefs keyed by type name
            string key = $"ASDBG_show_{t.FullName}_{timing}";
            bool current = EditorPrefs.GetBool(key, false);
            EditorPrefs.SetBool(key, !current);
        }
        if (GUILayout.Button("Unsub All", GUILayout.Width(72)))
        {
            if (EditorUtility.DisplayDialog("Unsubscribe", $"Remove all subscribers for {t.Name} ({timing})?", "Yes", "No"))
            {
                // attempt to remove by invoking unsubscribe with each delegate (best-effort)
                try
                {
                    foreach (var del in delegates.ToList())
                    {
                        ActionSystem.Instance.UnsubscribeReaction(t, del as Action<GameAction>, timing);
                    }
                    RefreshNow();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to unsubscribe all: {ex}");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // show delegate details if key toggled
        string showKey = $"ASDBG_show_{t.FullName}_{timing}";
        if (EditorPrefs.GetBool(showKey, false))
        {
            foreach (var d in delegates)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(12);
                var m = d?.Method;
                var tgt = d?.Target;
                EditorGUILayout.LabelField(m != null ? $"{m.DeclaringType?.FullName}.{m.Name}" : "<null>", GUILayout.MaxWidth(420));
                EditorGUILayout.LabelField(tgt != null ? tgt.ToString() : "<static>", GUILayout.MaxWidth(220));
                if (GUILayout.Button("Ping", GUILayout.Width(40)))
                {
                    if (tgt is UnityEngine.Object uo) EditorGUIUtility.PingObject(uo);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.44f));
        EditorGUILayout.LabelField("Call Stack & Dead Loop", EditorStyles.boldLabel);

        rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

        showCallStack = EditorGUILayout.Foldout(showCallStack, $"_callStack ({callStackSnapshot.Count})");
        if (showCallStack)
        {
            if (callStackSnapshot.Count == 0)
            {
                EditorGUILayout.HelpBox("Call stack is empty", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField("Types currently in call-stack (HashSet, no guaranteed order):", EditorStyles.miniLabel);
                foreach (var t in callStackSnapshot)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(t.FullName);
                    if (GUILayout.Button("Select Type", GUILayout.Width(80)))
                    {
                        // try to ping script asset if exists
                        var monoScripts = MonoImporter.GetAllRuntimeMonoScripts();
                        foreach (var ms in monoScripts)
                        {
                            if (ms.GetClass() == t)
                            {
                                Selection.activeObject = ms;
                                break;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        showDeadLoopLog = EditorGUILayout.Foldout(showDeadLoopLog, $"Dead-loop Events ({deadLoopEvents.Count})");
        if (showDeadLoopLog)
        {
            if (deadLoopEvents.Count == 0)
            {
                EditorGUILayout.HelpBox("No dead-loop errors captured in this session.", MessageType.Info);
            }
            else
            {
                foreach (var e in deadLoopEvents)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"Time: {e.Time:HH:mm:ss}  Type: {e.TypeName}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(e.Message);
                    if (GUILayout.Button("Show StackTrace"))
                    {
                        var win = EditorWindow.GetWindow<StackTraceViewerWindow>(true, "DeadLoop StackTrace", true);
                        win.SetContent(e.StackTrace);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawBottomPanel()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Utilities / Quick Test", EditorStyles.boldLabel);

        bottomScroll = EditorGUILayout.BeginScrollView(bottomScroll, GUILayout.Height(110));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Perform GameAction (parameterless ctor types)", GUILayout.Width(300));
        selectedActionTypeIndex = EditorGUILayout.Popup(selectedActionTypeIndex, actionTypeNames ?? new string[] { "<None>" });
        if (GUILayout.Button("Perform", GUILayout.Width(80)))
        {
            TryPerformSelectedAction();
        }
        if (GUILayout.Button("Ping Type Script", GUILayout.Width(110)))
        {
            PingSelectedActionTypeScript();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Open ActionSystem Source (refl info)", GUILayout.Width(260)))
        {
            // just show some info
            Debug.Log($"ActionSystem reflection fields: preSubs={fi_preSubs != null}, postSubs={fi_postSubs != null}, perfSubs={fi_perfSubs != null}, callStack={fi_callStack != null}");
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void RefreshActionTypeList()
    {
        // Use TypeCache if available
#if UNITY_2020_1_OR_NEWER
        try
        {
            var types = UnityEditor.TypeCache.GetTypesDerivedFrom<GameAction>().Where(t => !t.IsAbstract).ToArray();
            actionTypes = types;
        }
        catch
        {
            actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => SafeGetTypes(a))
                .Where(t => typeof(GameAction).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
        }
#else
        actionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => SafeGetTypes(a))
            .Where(t => typeof(GameAction).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();
#endif
        actionTypeNames = actionTypes?.Select(t => t.FullName).ToArray() ?? new string[] { "<None>" };
        if (actionTypeNames.Length == 0) actionTypeNames = new string[] { "<None>" };
        selectedActionTypeIndex = Mathf.Clamp(selectedActionTypeIndex, 0, Math.Max(0, actionTypeNames.Length - 1));
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly asm)
    {
        try
        {
            return asm.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(x => x != null);
        }
        catch
        {
            return new Type[0];
        }
    }

    private void TryPerformSelectedAction()
    {
        if (actionTypes == null || actionTypes.Length == 0) return;
        var t = actionTypes[Mathf.Clamp(selectedActionTypeIndex, 0, actionTypes.Length - 1)];
        if (t == null) return;

        try
        {
            // try parameterless ctor
            var ctor = t.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                Debug.LogWarning($"Type {t.FullName} has no parameterless constructor, cannot create sample instance.");
                return;
            }
            var inst = (GameAction)Activator.CreateInstance(t);
            ActionSystem.Instance.PerformGameAction(inst);
            Debug.Log($"Performed sample action: {t.FullName}");
            RefreshNow();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to Perform action {t.FullName}: {ex}");
        }
    }

    private void PingSelectedActionTypeScript()
    {
        if (actionTypes == null || actionTypes.Length == 0) return;
        var t = actionTypes[Mathf.Clamp(selectedActionTypeIndex, 0, actionTypes.Length - 1)];
        if (t == null) return;
        var ms = MonoImporter.GetAllRuntimeMonoScripts().FirstOrDefault(s => s.GetClass() == t);
        if (ms != null) Selection.activeObject = ms;
        else Debug.LogWarning("No MonoScript asset found for type: " + t.FullName);
    }

    private class DeadLoopEvent
    {
        public DateTime Time;
        public string Message;
        public string TypeName;
        public string StackTrace;
    }
}

/// <summary>
/// Small helper window to show stack trace content.
/// </summary>
public class StackTraceViewerWindow : EditorWindow
{
    private string content;
    private Vector2 scroll;
    public void SetContent(string s)
    {
        content = s;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Stack Trace", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(content ?? "<empty>", GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Close")) Close();
    }
}