using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Microsoft.Win32;

namespace tasks
{
    internal class Program
    {
        #region SetTask
        static string[] SetParams()
        {
            string[] statuses = { "предстоит", "в процессе", "завершено" };

            Console.Write("Введите имя задачи: ");
            string name = Console.ReadLine();
            if (name == "0") return null;

            Console.Write("Введите описание задачи: ");
            string description = Console.ReadLine();
            if (description == "0") return null;

            string status = "";
            while (!statuses.Contains(status.ToLower()))
            {
                Console.Write("Введите статус задачи (Предстоит, В процессе, Завершено): ");
                status = Console.ReadLine();
                if (status == "0") return null;
                Console.WriteLine();
            }

            return new string[] { name, description, status.ToUpper() };
        }

        static void SetSubTasks(XmlDocument xmlDoc, XmlElement newTask)
        {
            string command = "";

            while (command != "0")
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
            if (task == null)
            {
                Console.WriteLine("Процесс заполнения задачи прерван!");
                return;
            }
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

        static void AddSubTask(XmlDocument xmlDoc, XmlNode taskNode, string subTaskName)
        {
            XmlElement subTask = xmlDoc.CreateElement("SubTask");

            XmlNode lastSubTask = taskNode.SelectSingleNode("SubTask[last()]");
            int id = (lastSubTask != null) ? int.Parse(lastSubTask.Attributes["id"].Value) + 1 : 0;
            subTask.SetAttribute("id", id.ToString());

            subTask.InnerText = subTaskName;
            taskNode.AppendChild(subTask);
        }
        #endregion

        #region GetTasks
        static void GetTasks()
        {
            XmlDocument xmlDoc = LoadExistingXmlDocument();

            if (xmlDoc != null)
            {
                DisplayAllTasks(xmlDoc);
            }
            else
            {
                Console.WriteLine("XML-документ не найден.");
            }
        }

        static void DisplayAllTasks(XmlDocument xmlDoc)
        {
            XmlNodeList taskNodes = xmlDoc.SelectNodes("//Task");

            Console.WriteLine("\nЗаписи: ");
            Console.WriteLine(new string('-', 90));
            if (taskNodes != null)
            {
                var sortedTaskNodes = taskNodes.Cast<XmlNode>()
                    .OrderByDescending(taskNode => taskNode.SelectSingleNode("status")?.InnerText == "В ПРОЦЕССЕ")
                    .ThenByDescending(taskNode => taskNode.SelectSingleNode("status")?.InnerText == "ПРЕДСТОИТ")
                    .ThenByDescending(taskNode => taskNode.SelectSingleNode("status")?.InnerText == "ЗАВЕРШЕНО");

                foreach (XmlNode taskNode in sortedTaskNodes)
                {
                    string id = taskNode.Attributes["id"].Value;
                    string name = taskNode.SelectSingleNode("name")?.InnerText;
                    string description = taskNode.SelectSingleNode("description")?.InnerText;
                    string status = taskNode.SelectSingleNode("status")?.InnerText;

                    Console.WriteLine($"{name} с id - {id}. Статус - {status}");
                    Console.WriteLine($"\t{description}");

                    XmlNodeList subTaskNodes = taskNode.SelectNodes("SubTask");
                    if (subTaskNodes != null)
                    {
                        foreach (XmlNode subTaskNode in subTaskNodes)
                        {
                            string subTaskId = subTaskNode.Attributes["id"]?.Value;
                            Console.WriteLine($"\t\tПодзадача с id - {subTaskId}:\t{subTaskNode.InnerText}");
                        }
                    }

                    Console.WriteLine(new string('-', 90));
                }
            }
            else
            {
                Console.WriteLine("Задачи не найдены.");
            }
        }
        #endregion

        #region RemoveTask
        static void RemoveTask()
        {
            XmlDocument xmlDoc = LoadExistingXmlDocument();

            Console.Write("Введите id задачи для удаления (end для отменя): ");
            string taskId = Console.ReadLine();

            if (taskId == "end")
            {
                Console.WriteLine("Отмена");
                return;
            }

        conf:
            Console.Write("Вы уверены в удалениии (1/0)");
            string conf = Console.ReadLine();
            if (conf == "0")
            {
                Console.WriteLine("Отмена");
                return;
            }
            else if (conf == "1")
            {
                Console.WriteLine("Удаление подтверждено");
            }
            else
            {
                Console.WriteLine("Команда не принята. Повторите попытку");
                goto conf;
            }

            XmlNode taskNodeToRemove = xmlDoc.SelectSingleNode($"//Task[@id='{taskId}']");

            if (taskNodeToRemove != null)
            {
                XmlNode parentNode = taskNodeToRemove.ParentNode;
                parentNode?.RemoveChild(taskNodeToRemove);
                xmlDoc.Save("tasks.xml");

                Console.WriteLine($"Задача с id {taskId} успешно удалена.");
            }
            else
            {
                Console.WriteLine($"Задача с id {taskId} не найдена.");
            }
        }
        #endregion

        #region ChangeTask
        static void ChangeTask()
        {
            XmlDocument xmlDoc = LoadExistingXmlDocument();

            Console.Write("Введите id задачи для изменения (end для отмены): ");
            string taskId = Console.ReadLine();

            if (taskId == "end")
            {
                Console.WriteLine("Отмена");
                return;
            }

            XmlNode taskNodeToChange = xmlDoc.SelectSingleNode($"//Task[@id='{taskId}']");

            if (taskNodeToChange != null)
            {
                Console.WriteLine($"Выбрана задача с id {taskId}.");

            action:
                Console.Write("Выберите действие (name, description, status, addSubTask, removeSubTask, end): ");
                string action = Console.ReadLine();

                switch (action)
                {
                    case "name":
                        Console.Write("Введите новое имя: ");
                        string newName = Console.ReadLine();
                        if (newName == "0")
                        {
                            Console.WriteLine("Отмена действий");
                            return;
                        }
                        taskNodeToChange.SelectSingleNode("name").InnerText = newName;
                        break;

                    case "description":
                        Console.Write("Введите новое описание: ");
                        string newDescription = Console.ReadLine();
                        if (newDescription == "0")
                        {
                            Console.WriteLine("Отмена действий");
                            return;
                        }
                        taskNodeToChange.SelectSingleNode("description").InnerText = newDescription;
                        break;

                    case "status":
                        string[] statuses = { "предстоит", "в процессе", "завершено" };
                        string status = "";
                        while (!statuses.Contains(status.ToLower()))
                        {
                            Console.Write("Введите статус задачи (Предстоит, В процессе, Завершено): ");
                            status = Console.ReadLine();
                            if (status == "0") return;
                            Console.WriteLine();
                        }
                        taskNodeToChange.SelectSingleNode("status").InnerText = status.ToUpper();
                        break;

                    case "addSubTask":
                        Console.Write("Введите новую подзадачу: ");
                        string newSubTask = Console.ReadLine();
                        if (newSubTask == "0")
                        {
                            Console.WriteLine("Отмена действий");
                            return;
                        }
                        AddSubTask(xmlDoc, taskNodeToChange, newSubTask);
                        break;

                    case "removeSubTask":
                        Console.Write("Введите id подзадачи для удаления: ");
                        string subTaskIdToRemove = Console.ReadLine();

                    conf:
                        Console.Write("Вы уверены в удалении (1/0): ");
                        string conf = Console.ReadLine();
                        if (conf == "0")
                        {
                            Console.WriteLine("Отмена");
                            return;
                        }
                        else if (conf == "1")
                        {
                            Console.WriteLine("Удаление подтверждено");
                        }
                        else
                        {
                            Console.WriteLine("Команда не принята. Повторите попытку");
                            goto conf;
                        }

                        RemoveSubTask(xmlDoc, taskNodeToChange, subTaskIdToRemove);
                        break;

                    case "end":
                        Console.WriteLine("Отмена");
                        return;

                    default:
                        Console.WriteLine("Некорректное действие. Отмена.");
                        goto action;
                }

                xmlDoc.Save("tasks.xml");

                Console.WriteLine("Изменения успешно сохранены.");
            }
            else
            {
                Console.WriteLine($"Задача с id {taskId} не найдена.");
            }
        }

        static void RemoveSubTask(XmlDocument xmlDoc, XmlNode taskNode, string subTaskId)
        {
            XmlNode subTaskNodeToRemove = taskNode.SelectSingleNode($"SubTask[@id='{subTaskId}']");

            if (subTaskNodeToRemove != null)
            {
                taskNode.RemoveChild(subTaskNodeToRemove);
                Console.WriteLine($"Подзадача с id {subTaskId} успешно удалена.");
            }
            else
            {
                Console.WriteLine($"Подзадача с id {subTaskId} не найдена.");
            }
        }
        #endregion

        #region RemoveFile
        static void RemoveFile()
        {
            Console.Write("Вы уверены что хотети удалить все данны (y/n): ");
            string answ = Console.ReadLine();

            if (answ.ToLower() == "y")
            {
                Console.WriteLine("Удаление");
                File.Delete("tasks.xml");
                Console.WriteLine("Удалено");
            }
            else
            {
                Console.WriteLine("Отмена");
            }
        }
        #endregion

        #region Base
        static void Menu()
        {
            Console.WriteLine("Доступные команды:");
            Console.WriteLine("0  - Завершение работы");
            Console.WriteLine("1  - Просмотр команд");
            Console.WriteLine("2  - Посмотреть все записи");
            Console.WriteLine("3  - Добавить запись");
            Console.WriteLine("4  - Редактировать запись");
            Console.WriteLine("5  - Удалить запись");
            Console.WriteLine("6  - Полная очистка");
            Console.WriteLine("7  - Путь к базовой папке");

            Console.WriteLine();
        }

        static void Commmand(int command)
        {
            switch (command)
            {
                case 0:
                    Console.WriteLine("Заверщение работы команды");
                    Console.WriteLine();
                    break;
                case 1:
                    Menu();
                    Console.WriteLine();
                    break;
                case 2:
                    GetTasks();
                    Console.WriteLine();
                    break;
                case 3:
                    SetTask();
                    Console.WriteLine();
                    break;
                case 4:
                    ChangeTask();
                    Console.WriteLine();
                    break;
                case 5:
                    RemoveTask();
                    Console.WriteLine();
                    break;
                case 6:
                    RemoveFile();
                    Console.WriteLine();
                    break;
                case 7:
                    Console.WriteLine("Путь к программе: " + Directory.GetCurrentDirectory());
                    Console.WriteLine();
                    break;
                default:
                    Console.WriteLine("Неверная команда. Повторите попытку");
                    Console.WriteLine();
                    break;
            }
        }

        #endregion

        static void Main(string[] args)
        {
            int actionCode;

            actionCode = 1;

            Commmand(actionCode);


            while (actionCode != 0)
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

        }
    }
}