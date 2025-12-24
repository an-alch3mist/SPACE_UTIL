using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LOOPLanguage
{
    /// <summary>
    /// Implements all game-specific built-in functions
    /// Functions that yield (take time) return IEnumerator
    /// Query functions return instantly
    /// </summary>
    public class GameBuiltinMethods
    {
        #region Movement Functions
        
        /// <summary>
        /// Moves the player in the specified direction
        /// </summary>
        public IEnumerator Move(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("move() takes exactly 1 argument");
            }
            
            string direction = args[0].ToString().ToLower();
            
            // Map direction constants to actual directions
            switch (direction)
            {
                case "up":
                case "north":
                    Debug.Log("Moving North");
                    break;
                case "down":
                case "south":
                    Debug.Log("Moving South");
                    break;
                case "right":
                case "east":
                    Debug.Log("Moving East");
                    break;
                case "left":
                case "west":
                    Debug.Log("Moving West");
                    break;
                default:
                    throw new RuntimeError($"Invalid direction: {direction}");
            }
            
            // Simulate movement animation
            yield return new WaitForSeconds(0.3f);
        }
        
        #endregion
        
        #region Farming Functions
        
        /// <summary>
        /// Harvests the entity at current position
        /// </summary>
        public IEnumerator Harvest()
        {
            Debug.Log("Harvesting");
            yield return new WaitForSeconds(0.2f);
        }
        
        /// <summary>
        /// Plants an entity at current position
        /// </summary>
        public IEnumerator Plant(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("plant() takes exactly 1 argument");
            }
            
            string entity = args[0].ToString();
            Debug.Log($"Planting {entity}");
            yield return new WaitForSeconds(0.3f);
        }
        
        /// <summary>
        /// Tills the ground at current position
        /// </summary>
        public IEnumerator Till()
        {
            Debug.Log("Tilling ground");
            yield return new WaitForSeconds(0.1f);
        }
        
        #endregion
        
        #region Query Functions (Instant - No Yield)
        
        /// <summary>
        /// Returns true if can harvest at current position
        /// </summary>
        public object CanHarvest()
        {
            // Simulate game logic
            return true;
        }
        
        /// <summary>
        /// Returns the ground type at current position
        /// </summary>
        public object GetGroundType()
        {
            // Simulate returning ground type
            return Grounds.Soil;
        }
        
        /// <summary>
        /// Returns the entity type at current position
        /// </summary>
        public object GetEntityType()
        {
            // Simulate returning entity or None
            return null;
        }
        
        /// <summary>
        /// Returns current X position
        /// </summary>
        public object GetPosX()
        {
            return 0.0;
        }
        
        /// <summary>
        /// Returns current Y position
        /// </summary>
        public object GetPosY()
        {
            return 0.0;
        }
        
        /// <summary>
        /// Returns world grid size
        /// </summary>
        public object GetWorldSize()
        {
            return 10.0;
        }
        
        /// <summary>
        /// Returns water level at current position
        /// </summary>
        public object GetWater()
        {
            return 0.5;
        }
        
        #endregion
        
        #region Inventory Functions
        
        /// <summary>
        /// Returns quantity of specified item
        /// </summary>
        public object NumItems(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("num_items() takes exactly 1 argument");
            }
            
            string item = args[0].ToString();
            // Simulate inventory lookup
            return 0.0;
        }
        
        /// <summary>
        /// Uses/consumes one unit of specified item
        /// </summary>
        public IEnumerator UseItem(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("use_item() takes exactly 1 argument");
            }
            
            string item = args[0].ToString();
            Debug.Log($"Using item: {item}");
            yield return new WaitForSeconds(0.1f);
        }
        
        #endregion
        
        #region Utility Functions
        
        /// <summary>
        /// Player performs a flip animation
        /// </summary>
        public IEnumerator DoAFlip()
        {
            Debug.Log("Doing a flip!");
            yield return new WaitForSeconds(1.0f);
        }
        
        /// <summary>
        /// Returns true if (x + y) is even
        /// </summary>
        public object IsEven(List<object> args)
        {
            if (args.Count != 2)
            {
                throw new RuntimeError("is_even() takes exactly 2 arguments");
            }
            
            int x = (int)(double)args[0];
            int y = (int)(double)args[1];
            return (x + y) % 2 == 0;
        }
        
        /// <summary>
        /// Returns true if (x + y) is odd
        /// </summary>
        public object IsOdd(List<object> args)
        {
            if (args.Count != 2)
            {
                throw new RuntimeError("is_odd() takes exactly 2 arguments");
            }
            
            int x = (int)(double)args[0];
            int y = (int)(double)args[1];
            return (x + y) % 2 == 1;
        }
        
        #endregion
    }
}
