using Gameplay.Data;
using Gameplay.MVC.Interfaces;

namespace Gameplay.MVC.Model
{
    public class HeroCharacterModel : CombatantModel, IHeroCombatant
    {
        public HeroCharacterModel() { } // 允许创建空的模型，后续再绑定数据
        
        public HeroCharacterModel(HeroCharacterData data) 
        {
            BindData(data);
        }

        public void BindData(HeroCharacterData data)
        {
            base.BindData(data, CombatantType.Hero);
        }
    }
}
