namespace Nez.Samples
{
    public class ManaComponent : Component
    {
        public int mana;
        public string playerName;
        
        public ManaComponent(string name)
        {
            this.playerName = name;
            this.mana = 5;
        }
    }
}