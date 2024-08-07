using ProjectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProjectManager
{
    internal class Task
    {
        readonly private string ID;
        private int ExecutionTime;
        List<string>  Dependencies = new List<string>();

        public Task(string id, int executionTime, List<string> dependencies)
        {
            ID = $"{id}";
            ExecutionTime = executionTime;
            Dependencies = dependencies;
        }
        public string GetID { get { return ID; } }
        public int TaskExecutionTime { get { return ExecutionTime; } set { ExecutionTime = value; } }
        public List<string> SeeDependencies { get { return Dependencies; } }
        //public List<string> SetDependencies { get { return Dependencies; } set { Dependencies = value; } }

        // Allows execution time to be edited. 
        public void edit(int executionTime)
        {
            TaskExecutionTime = executionTime;
        }
        /// <summary>
        /// Dictates how the task will be written to a string. Especially when saving to the save file.
        /// </summary>
        /// <returns></returns>
        public string ToString()
        {
            // If there are dependencies with the task,output will include commas in appropriate areas between dependent tasks.
            if (Dependencies.Count > 0)
            {
                string dependencies ="";
                for (int i = 0; i < Dependencies.Count; i++) {
                    if (i == 0)
                    {
                        // if the first dependency is being read, do not place comma in front of it.
                        dependencies = Dependencies[0];
                        continue;
                    }
                    if (Dependencies[i] == null)
                    {
                        continue;
                    }
                    dependencies = dependencies + ", " + Dependencies[i];
                }
                if (dependencies == null)
                {
                    return $"{ID}, {ExecutionTime}";
                }
                return $"{ID}, {ExecutionTime}, {dependencies} ";
            }
            // Output will only include one comma if there are no dependencies. 
            else
            {
                return $"{ID}, {ExecutionTime}";
            }
        }
    }
}
