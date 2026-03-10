using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Conversion.Hytale.PlayerEntities
{
    public class hy_Inventory
    {
        public int Version { get; set; } = 4;
        public hy_InventoryStorage Storage { get; set; } = new hy_InventoryStorage()
        {
            Capacity = 36
        };
        public hy_InventoryStorage Armor { get; set; } = new hy_InventoryStorage()
        {
            Capacity = 4
        };
        public hy_InventoryStorage HotBar { get; set; } = new hy_InventoryStorage()
        {
            Capacity = 9
        };
        public hy_InventoryStorage Utility { get; set; } = new hy_InventoryStorage()
        {
            Capacity = 4
        };
        public hy_InventoryStorage Backpack { get; set; } = new hy_InventoryStorage()
        {
            Id = "Empty"
        };
        public hy_InventoryStorage Tool { get; set; } = new hy_InventoryStorage()
        {
            Capacity = 23,
            Items = new Dictionary<int, hy_Item>()
            {
                { 
                    0, 
                    new hy_Item()
                    {
                        Id = "EditorTool_Paint",
                        Quantity = 1
                    } 
                },
                {
                    1,
                    new hy_Item()
                    {
                        Id = "EditorTool_Selection",
                        Quantity = 1
                    }
                },
                {
                    2,
                    new hy_Item()
                    {
                        Id = "EditorTool_Line",
                        Quantity = 1
                    }
                },
                {
                    3,
                    new hy_Item()
                    {
                        Id = "EditorTool_Sculpt",
                        Quantity = 1
                    }
                },
                {
                    4,
                    new hy_Item()
                    {
                        Id = "EditorTool_Paste",
                        Quantity = 1
                    }
                },
                {
                    5,
                    new hy_Item()
                    {
                        Id = "EditorTool_Layers",
                        Quantity = 1
                    }
                }
            }
        };
        public int ActiveHotbarSlot { get; set; } = 0;
        public int ActiveToolSlot { get; set; } = -1;
        public int ActiveUtilitySlot { get; set; } = -1;
        public string SortType { get; set; } = "Name";
    }
}
