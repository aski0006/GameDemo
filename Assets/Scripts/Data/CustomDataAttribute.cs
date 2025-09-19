using System;
using UnityEngine;

namespace AsakiFramework.Data
{
    /// <summary>
    /// 自定义数据特性 - 用于标记可序列化的自定义数据类型
    /// 该特性可以帮助编辑器识别和处理特殊的游戏数据
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class CustomDataAttribute : Attribute
    {
        /// <summary>
        /// 数据分类，用于在编辑器中分组显示
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 数据描述，用于说明数据的用途
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否可编辑，控制数据在编辑器中是否可修改
        /// </summary>
        public bool IsEditable { get; set; } = true;

        /// <summary>
        /// 排序顺序，用于控制数据显示的顺序
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CustomDataAttribute()
        {
        }

        /// <summary>
        /// 带分类的构造函数
        /// </summary>
        /// <param name="category">数据分类</param>
        public CustomDataAttribute(string category)
        {
            Category = category;
        }

        /// <summary>
        /// 带分类和描述的构造函数
        /// </summary>
        /// <param name="category">数据分类</param>
        /// <param name="description">数据描述</param>
        public CustomDataAttribute(string category, string description)
        {
            Category = category;
            Description = description;
        }
    }
}
