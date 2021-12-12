using System;
using ConsoleMenu;

namespace OZI_Lab5
{
    class Program
    {
        static void Main() => new Menu("Хеширование сообщений, генерация и проверка электронной подписи", nameof(OZI_Lab5)).Execute();
    }
}
