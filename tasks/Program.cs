using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace tasks
{
    internal class Program
    {
        static void Menu()
        {
            Console.WriteLine("Доступные команды:");
            Console.WriteLine("0 - Завершение работы");
            Console.WriteLine("1 - Просмотр команд");
            Console.WriteLine("2 - Посмотреть все записи");
            Console.WriteLine("3 - Добавить запись");
            Console.WriteLine("4 - Редактировать запись");
            Console.WriteLine("5 - Удалить запись");
            Console.WriteLine();
        }

        #region SetTask
        static string[] SetParams()
        {
            string[] statuses = { "предстоит", "в процессе", "завершено" };

            Console.Write("Введите имя задачи: ");
            string name = Console.ReadLine();

            Console.Write("Введите описание задачи: ");
            string description = Console.ReadLine();

            string status = "";
            while (!statuses.Contains(status.ToLower()))
            {
                Console.Write("Введите статус задачи (Предстоит, В процессе, Завершено): ");
                status = Console.ReadLine();
                Console.WriteLine();
            }

            return new string[] { name, description, status.ToUpper() };
        }

        static void SetSubTasks(XmlDocument xmlDoc, XmlElement newTask)
        {
            string command = "";
            while(command != "0")
            {
                Console.Write("Введите подзадачу или 0 для завершения: ");
                command = Console.ReadLine();
                if (command != "0")
                {
                    AddSubTask(xmlDoc, newTask, command);
                    Console.WriteLine("Подзадача внесена!");
                }
            }
        }

        static void SetTask()
        {
            XmlDocument xmlDoc = LoadExistingXmlDocument() ?? CreateNewXmlDocument();

            string[] task = SetParams();
            XmlElement newTask = CreateTask(xmlDoc, task[0], task[1], task[2]);

            SetSubTasks(xmlDoc, newTask);

            xmlDoc.DocumentElement?.AppendChild(newTask);

            xmlDoc.Save("tasks.xml");

            Console.WriteLine("Новая задача успешно добавлена.\n");
        }

        static XmlDocument LoadExistingXmlDocument()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("tasks.xml");
                return xmlDoc;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }

        static XmlDocument CreateNewXmlDocument()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("Tasks");
            xmlDoc.AppendChild(root);
            return xmlDoc;
        }

        static XmlElement CreateTask(XmlDocument xmlDoc, string name, string description, string status)
        {
            XmlElement task = xmlDoc.CreateElement("Task");

            // Генерируем id для задачи, основываясь на предыдущей задаче (если она есть)
            XmlNode lastTask = xmlDoc.SelectSingleNode("//Task[last()]");
            int id = (lastTask != null) ? int.Parse(lastTask.Attributes["id"].Value) + 1 : 0;
            task.SetAttribute("id", id.ToString());

            XmlElement nameElement = xmlDoc.CreateElement("name");
            nameElement.InnerText = name;
            task.AppendChild(nameElement);

            XmlElement descElement = xmlDoc.CreateElement("description");
            descElement.InnerText = description;
            task.AppendChild(descElement);

            XmlElement statusElement = xmlDoc.CreateElement("status");
            statusElement.InnerText = status;
            task.AppendChild(statusElement);

            return task;
        }

        static void AddSubTask(XmlDocument xmlDoc, XmlElement task, string subTaskName)
        {
            XmlElement subTask = xmlDoc.CreateElement("SubTask");
            subTask.InnerText = subTaskName;
            task.AppendChild(subTask);
        }

        #endregion

        static void Commmand(int command)
        {
            switch (command)
            {
                case 0:
                    Console.WriteLine("Заверщение работы команды");
                    break;
                case 1:
                    Menu();
                    break;
                case 3:
                    SetTask();
                    break;
                default:
                    Console.WriteLine("Неверная команда. Повторите попытку");
                    break;
                   
            }
        }


        

        static void Main(string[] args)
        {
            int actionCode = 1;

            while(actionCode != 0)
            {
                Console.Write("Введите код команды: ");
                if (int.TryParse(Console.ReadLine(), out int ac))
                {
                    actionCode = ac;
                    Commmand(actionCode);
                }
                else
                {
                    Console.WriteLine("Неверная команда. Для просмотра команд введите 1");
                }
            }
            Environment.Exit(0);
        }
    }
}
