using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class ParallelExamples
{
    static void Main()
    {
        Console.WriteLine("=== Класс Parallel - Параллельные циклы и операции ===");

        // Класс Parallel предоставляет удобные методы для параллельного выполнения циклов
        // Он автоматически распределяет работу по нескольким потокам

        ParallelForExample();           // Параллельные версии цикла for
        ParallelForEachExample();       // Параллельные версии цикла foreach  
        ParallelInvokeExample();        // Параллельное выполнение нескольких действий
        ParallelBreakExample();         // Прерывание параллельных операций
        ParallelWithStateExample();     // Работа с локальным состоянием в циклах
        ParallelWithOptionsExample();   // Настройка параметров параллелизма

        Console.ReadLine();
    }

    static void ParallelForExample()
    {
        Console.WriteLine("\n--- 1. Parallel.For - Параллельный аналог for ===");

        // Parallel.For распределяет итерации цикла между несколькими потоками
        // Простой пример: выводим номера итераций и ID потоков
        Console.WriteLine("Простой Parallel.For:");
        Parallel.For(0, 10, i =>
        {
            // Этот код выполняется параллельно в разных потоках
            Console.WriteLine($"  Итерация i = {i}, выполняется в потоке {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(100); // Небольшая задержка для наглядности
        });

        // Parallel.For с шагом (аналог for с increment)
        Console.WriteLine("\nParallel.For с шагом 2:");
        Parallel.For(0, 20, 2, i => // Третий параметр - шаг (increment)
        {
            Console.WriteLine($"  Четное i = {i}");
        });

        // Parallel.For с локальным состоянием - сложный но мощный механизм
        Console.WriteLine("\nParallel.For с локальным состоянием (суммирование):");
        long totalSum = 0; // Общий итог

        Parallel.For(0, 100, // от 0 до 99

            // Функция инициализации локального состояния для КАЖДОГО потока
            () => 0L, // Каждый поток начинает с локальной суммы = 0

            // Тело цикла: (текущий индекс, состояние цикла, локальная сумма) => новая локальная сумма
            (i, loopState, localSum) =>
            {
                // Каждый поток накапливает сумму в своей локальной переменной
                localSum += i; // Добавляем текущее число к локальной сумме
                return localSum; // Возвращаем обновленную локальную сумму
            },

            // Функция финализации: вызывается для КАЖДОГО потока при завершении
            localSum =>
            {
                // Объединяем локальные суммы всех потоков в общий результат
                // Interlocked обеспечивает атомарность операции для многопоточности
                Interlocked.Add(ref totalSum, localSum);
                Console.WriteLine($"  Поток {Thread.CurrentThread.ManagedThreadId} внес вклад: {localSum}");
            }
        );

        Console.WriteLine($"✓ Итоговая сумма чисел от 0 до 99: {totalSum}");

        // Проверяем правильность: сумма арифметической прогрессии
        long expectedSum = 99 * 100 / 2; // n*(n+1)/2
        Console.WriteLine($"  Ожидаемая сумма: {expectedSum}, корректно: {totalSum == expectedSum}");
    }

    static void ParallelForEachExample()
    {
        Console.WriteLine("\n--- 2. Parallel.ForEach - Параллельный аналог foreach ===");

        // Parallel.ForEach распределяет обработку элементов коллекции между потоками
        var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        Console.WriteLine("Обработка коллекции чисел:");
        Parallel.ForEach(numbers, number =>
        {
            Console.WriteLine($"  Обрабатывается число {number} в потоке {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(200); // Имитация обработки
        });

        // Parallel.ForEach с Partitioner для больших коллекций
        Console.WriteLine("\nParallel.ForEach с Partitioner (оптимизация для больших данных):");
        var largeCollection = Enumerable.Range(1, 1000); // 1000 элементов

        // Partitioner автоматически разбивает данные на оптимальные блоки
        // Это уменьшает накладные расходы на синхронизацию
        Parallel.ForEach(largeCollection, new ParallelOptions { MaxDegreeOfParallelism = 4 },
            item =>
            {
                // Имитация CPU-интенсивной операции
                double result = Math.Sqrt(item) * Math.Log(item + 1);
                // В реальном коде здесь была бы полезная работа
            });

        Console.WriteLine("✓ Parallel.ForEach с Partitioner завершен");
    }

    static void ParallelInvokeExample()
    {
        Console.WriteLine("\n--- 3. Parallel.Invoke - Параллельное выполнение действий ===");

        // Parallel.Invoke выполняет несколько действий параллельно
        // Полезно когда нужно запустить несколько независимых операций

        Console.WriteLine("Запускаю 4 независимых действия параллельно:");
        Parallel.Invoke(
            () =>
            {
                Console.WriteLine($"  🎵 Действие 1 началось в потоке {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(1000); // Самое долгое действие - 1 секунда
                Console.WriteLine("  Действие 1: Завершено (обработка музыки)");
            },
            () =>
            {
                Console.WriteLine($"  📷 Действие 2 началось в потоке {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(800); // 0.8 секунды
                Console.WriteLine("  Действие 2: Завершено (обработка изображений)");
            },
            () =>
            {
                Console.WriteLine($"  📊 Действие 3 началось в потоке {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(600); // 0.6 секунды
                Console.WriteLine("  Действие 3: Завершено (анализ данных)");
            },
            () =>
            {
                Console.WriteLine($"  🔍 Действие 4 началось в потоке {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(400); // Самое быстрое действие - 0.4 секунды
                Console.WriteLine("  Действие 4: Завершено (поиск информации)");
            }
        );

        Console.WriteLine("✓ Все действия Parallel.Invoke завершены!");
        Console.WriteLine("  При последовательном выполнении это заняло бы ~2.8 секунды");
        Console.WriteLine("  При параллельном - примерно 1 секунду (время самого долгого действия)");
    }

    static void ParallelBreakExample()
    {
        Console.WriteLine("\n--- 4. Прерывание параллельных операций ===");

        // ParallelLoopState позволяет управлять выполнением параллельного цикла
        // Break() - завершает цикл после текущих итераций
        // Stop() - немедленно останавливает цикл

        Console.WriteLine("Parallel.For с прерыванием (Break):");
        Parallel.For(0, 100, (i, loopState) =>
        {
            if (i >= 50)
            {
                Console.WriteLine($"  Достигнуто условие прерывания (i = {i})");
                loopState.Break(); // Завершаем цикл после текущих выполненных итераций
                // Текущие выполняемые итерации будут завершены, новые не начнутся
                return;
            }

            if (i % 10 == 0)
            {
                Console.WriteLine($"  Обработка i = {i}");
            }
        });

        Console.WriteLine("✓ Parallel.For с Break завершен");

        // Пример с Stop (немедленная остановка)
        Console.WriteLine("\nParallel.For с немедленной остановкой (Stop):");
        Parallel.For(0, 100, (i, loopState) =>
        {
            if (i >= 25)
            {
                Console.WriteLine($"  Экстренная остановка на i = {i}");
                loopState.Stop(); // Немедленная остановка цикла
                return;
            }

            Console.WriteLine($"  Быстрая обработка i = {i}");
        });
    }

    static void ParallelWithStateExample()
    {
        Console.WriteLine("\n--- 5. Работа с состоянием в параллельных циклах ===");

        // Частая задача: агрегация данных в параллельном цикле
        // Пример: поиск минимального значения в массиве

        int[] data = { 5, 2, 8, 1, 9, 3, 6, 4, 7 };
        int minValue = int.MaxValue; // Изначально устанавливаем максимально возможное значение

        Console.WriteLine($"Исходный массив: [{string.Join(", ", data)}]");

        Parallel.For(0, data.Length,
            // Инициализатор локального состояния для каждого потока
            () => int.MaxValue, // Каждый поток начинает с "бесконечного" минимума

            // Тело цикла: обрабатываем один элемент и обновляем локальное состояние
            (i, loopState, localMin) =>
            {
                // Каждый поток ищет минимум в своей порции данных
                int currentValue = data[i];
                if (currentValue < localMin)
                {
                    localMin = currentValue;
                    Console.WriteLine($"  Поток {Thread.CurrentThread.ManagedThreadId} нашел новый локальный минимум: {localMin}");
                }
                return localMin; // Возвращаем обновленный локальный минимум
            },

            // Финализатор: объединяем результаты всех потоков
            localMin =>
            {
                // Так как несколько потоков могут одновременно обновлять minValue,
                // используем lock для обеспечения потокобезопасности
                lock (data) // data используется как объект блокировки
                {
                    if (localMin < minValue)
                    {
                        Console.WriteLine($"  💡 Обновление глобального минимума: {minValue} -> {localMin}");
                        minValue = localMin;
                    }
                }
            }
        );

        Console.WriteLine($"✓ Найденный минимум: {minValue}");
        Console.WriteLine($"  Ожидаемый минимум: {data.Min()}, корректно: {minValue == data.Min()}");
    }

    static void ParallelWithOptionsExample()
    {
        Console.WriteLine("\n--- 6. Настройка параметров параллелизма ===");

        // ParallelOptions позволяет тонко настраивать поведение параллельных операций
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 2 // Ограничиваем количество одновременных потоков
            // TaskScheduler = ... // Можно указать планировщик задач
            // CancellationToken = ... // Можно добавить токен отмены
        };

        Console.WriteLine($"Parallel.For с ограничением: MaxDegreeOfParallelism = {options.MaxDegreeOfParallelism}");

        Parallel.For(0, 10, options, i =>
        {
            Console.WriteLine($"  Обработка i = {i}, поток {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(500); // Имитация работы

            // Выводим информацию о текущих потоках пула
            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
            Console.WriteLine($"    Доступно потоков в пуле: {workerThreads}");
        });

        Console.WriteLine("✓ Parallel.For с ограничением потоков завершен");
        Console.WriteLine("  Ограничение полезно когда:");
        Console.WriteLine("  - Нужно избежать перегрузки системы");
        Console.WriteLine("  - Работаем с ограниченными ресурсами (БД, сеть)");
        Console.WriteLine("  - Отлаживаем параллельный код");
    }
}