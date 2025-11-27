using System;
using System.Threading;
using System.Threading.Tasks;

class TaskExamples
{
    static void Main()
    {
        Console.WriteLine("=== Класс Task - Детальное изучение ===");

        // Task представляет асинхронную операцию, которая может выполняться в отдельном потоке
        // Это основная единица работы в TPL (Task Parallel Library)

        SimpleTaskExample();        // Базовое создание и запуск задач
        TaskWithResultExample();    // Работа с задачами, возвращающими значения
        TaskWaitingExample();       // Различные способы ожидания задач
        TaskExceptionHandling();    // Обработка ошибок в асинхронных операциях
        TaskContinuationExample();  // Цепочки задач и продолжения
        TaskCancellationExample();  // Механизм отмены длительных операций

        Console.ReadLine();
    }

    static void SimpleTaskExample()
    {
        Console.WriteLine("\n--- 1. Простые задачи - создание и запуск ---");

        // Способ 1: Task.Run - самый простой способ запустить задачу в пуле потоков
        // Пул потоков - это набор готовых к работе потоков, которые переиспользуются
        Task task1 = Task.Run(() =>
        {
            // Этот код выполняется в отдельном потоке из пула
            Console.WriteLine($"Задача 1: Выполняется в потоке {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(1000); // Имитация долгой работы (1 секунда)
            Console.WriteLine("Задача 1: Долгая операция завершена");
        });
        // Task.Run автоматически запускает задачу, не нужно вызывать Start()

        // Способ 2: Явное создание через конструктор + ручной запуск
        // Полезно когда нужно отложить запуск или настроить задачу перед выполнением
        Task task2 = new Task(() =>
        {
            Console.WriteLine($"Задача 2: Выполняется в потоке {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(500); // Полсекунды работы
            Console.WriteLine("Задача 2: Быстрая операция завершена");
        });
        task2.Start(); // Запускаем задачу явно

        // Ожидаем завершения обеих задач
        // WaitAll блокирует текущий поток до тех пор, пока ВСЕ задачи не завершатся
        Task.WaitAll(task1, task2);
        Console.WriteLine("✓ Все задачи завершены, продолжаем выполнение main потока");
    }

    static void TaskWithResultExample()
    {
        Console.WriteLine("\n--- 2. Задачи с возвращаемыми значениями ---");

        // Task<T> - обобщенная задача, которая возвращает результат типа T
        // В данном случае - Task<int> возвращает целое число
        Task<int> calculationTask = Task.Run(() =>
        {
            Console.WriteLine("Вычисление запущено...");
            Thread.Sleep(1000); // Имитируем сложные вычисления
            int result = 42;   // Легендарный "Ответ на главный вопрос жизни, вселенной и всего такого"
            Console.WriteLine($"Вычисление завершено, результат: {result}");
            return result;     // Возвращаем результат из задачи
        });

        // Свойство Result блокирует текущий поток до получения результата
        // Если задача еще не завершена, поток будет ждать
        int result = calculationTask.Result;
        Console.WriteLine($"Получен результат из задачи: {result}");

        // Пример с несколькими задачами, возвращающими разные результаты
        Task<string> helloTask = Task.Run(() =>
        {
            Thread.Sleep(300);
            return "Hello"; // Возвращаем строку
        });

        Task<string> worldTask = Task.Run(() =>
        {
            Thread.Sleep(200);
            return "World"; // Возвращаем другую строку
        });

        // Ждем завершения всех задач и используем их результаты
        Task.WaitAll(helloTask, worldTask);
        Console.WriteLine($"{helloTask.Result} {worldTask.Result} - результаты объединены!");
    }

    static void TaskWaitingExample()
    {
        Console.WriteLine("\n--- 3. Различные стратегии ожидания задач ---");

        // Создаем массив из 3 задач с разным временем выполнения
        var tasks = new Task[3];

        for (int i = 0; i < tasks.Length; i++)
        {
            int taskId = i; // Важно: захватываем копию i для замыкания
            tasks[i] = Task.Run(() =>
            {
                // Каждая задача спит разное количество времени
                int sleepTime = 1000 * (taskId + 1); // 1, 2, 3 секунды
                Console.WriteLine($"Задача {taskId}: начну спать {sleepTime}ms");
                Thread.Sleep(sleepTime);
                Console.WriteLine($"Задача {taskId}: проснулась!");
            });
        }

        // Стратегия 1: WaitAll - ждем ВСЕ задачи
        Console.WriteLine("Ожидаем завершения ВСЕХ задач...");
        Task.WaitAll(tasks);
        Console.WriteLine("✓ Все 3 задачи завершены!");

        // Стратегия 2: WaitAny - ждем ЛЮБУЮ задачу
        Task[] mixedTasks = new Task[]
        {
            Task.Run(() => {
                Thread.Sleep(1500);
                Console.WriteLine("Медленная задача завершилась");
            }),
            Task.Run(() => {
                Thread.Sleep(500);
                Console.WriteLine("Быстрая задача завершилась");
            })
        };

        Console.WriteLine("Ожидаем завершения ЛЮБОЙ задачи...");
        int firstCompletedIndex = Task.WaitAny(mixedTasks);
        Console.WriteLine($"Первой завершилась задача с индексом: {firstCompletedIndex}");

        // Стратегия 3: Wait с таймаутом - ждем, но не дольше указанного времени
        Task longRunningTask = Task.Run(() =>
        {
            Thread.Sleep(3000); // Задача работает 3 секунды
            Console.WriteLine("Долгая задача наконец-то завершилась");
        });

        Console.WriteLine("Ожидаем задачу максимум 1 секунду...");
        bool completedInTime = longRunningTask.Wait(1000); // Таймаут 1 секунда
        Console.WriteLine($"Задача завершилась в течение таймаута: {completedInTime}");

        if (!completedInTime)
        {
            Console.WriteLine("Задача все еще выполняется, но мы не будем ждать дальше");
        }
    }

    static void TaskExceptionHandling()
    {
        Console.WriteLine("\n--- 4. Обработка исключений в задачах ---");

        // Важно: исключения в задачах не выбрасываются сразу, а "оборачиваются" в AggregateException

        // Создаем задачу, которая гарантированно упадет с исключением
        Task faultyTask = Task.Run(() =>
        {
            Console.WriteLine("Проблемная задача начинается...");
            throw new InvalidOperationException("Имитация критической ошибки в задаче!");
            // Этот код никогда не выполнится:
            Console.WriteLine("Этот текст никогда не увидим");
        });

        try
        {
            // Wait() заставляет дождаться завершения задачи
            // Если в задаче было исключение, оно будет выброшено здесь
            faultyTask.Wait();
        }
        catch (AggregateException ae)
        {
            // AggregateException содержит ВСЕ исключения, произошедшие в задаче
            // (задача может порождать несколько исключений)
            Console.WriteLine("Поймано AggregateException с внутренними исключениями:");
            foreach (var innerException in ae.InnerExceptions)
            {
                Console.WriteLine($"  → {innerException.GetType().Name}: {innerException.Message}");
            }
        }

        // Альтернативный подход: проверка статуса задачи
        Task anotherFaultyTask = Task.Run(() =>
        {
            throw new ArgumentException("Еще одна ошибка для демонстрации");
        });

        // Даем задаче время завершиться (в реальном коде так не делайте!)
        Thread.Sleep(100);

        // Проверяем свойства задачи
        Console.WriteLine($"Статус задачи: {anotherFaultyTask.Status}");
        Console.WriteLine($"Задача завершена: {anotherFaultyTask.IsCompleted}");
        Console.WriteLine($"Задача упала: {anotherFaultyTask.IsFaulted}");
        Console.WriteLine($"Задача отменена: {anotherFaultyTask.IsCanceled}");

        if (anotherFaultyTask.IsFaulted)
        {
            Console.WriteLine($"Задача завершилась с ошибкой: {anotherFaultyTask.Exception.Message}");
        }
    }

    static void TaskContinuationExample()
    {
        Console.WriteLine("\n--- 5. Цепочки задач и продолжения ---");

        // ContinueWith позволяет создать цепочку: задача B запустится после завершения задачи A

        // Начальная задача возвращает число
        Task<int> initialTask = Task.Run(() =>
        {
            Console.WriteLine("Начальная задача: генерирую число...");
            Thread.Sleep(800);
            return 10; // Возвращаем результат
        });

        // Продолжение получает результат предыдущей задачи
        Task<string> continuationTask = initialTask.ContinueWith(previousTask =>
        {
            // previousTask - это завершенная начальная задача
            int previousResult = previousTask.Result; // Получаем результат предыдущей задачи
            int newResult = previousResult * 2;       // Обрабатываем результат
            return $"Удвоенный результат: {newResult}"; // Возвращаем новый результат
        });

        // Ждем завершения цепочки и выводим финальный результат
        Console.WriteLine($"Финальный результат: {continuationTask.Result}");

        // Продолжения с условиями выполнения
        Task<int> sourceTask = Task.Run(() =>
        {
            Console.WriteLine("Источник данных начинает работу...");
            Thread.Sleep(500);
            return 5;
        });

        // Продолжение выполнится ТОЛЬКО если предыдущая задача завершилась успешно
        sourceTask.ContinueWith(t =>
        {
            Console.WriteLine($"✓ Успешное завершение! Результат: {t.Result}");
        }, TaskContinuationOptions.OnlyOnRanToCompletion);

        // Продолжение выполнится ТОЛЬКО если предыдущая задача упала с ошибкой
        sourceTask.ContinueWith(t =>
        {
            // t.Exception будет содержать ошибку предыдущей задачи
            Console.WriteLine($"✗ Задача завершилась с ошибкой: {t.Exception}");
        }, TaskContinuationOptions.OnlyOnFaulted);

        // Продолжение выполнится В ЛЮБОМ случае (успех или ошибка)
        sourceTask.ContinueWith(t =>
        {
            Console.WriteLine("Задача завершена (независимо от результата)");
        }, TaskContinuationOptions.None);

        // Даем время для выполнения продолжений
        Thread.Sleep(1000);
    }

    static void TaskCancellationExample()
    {
        Console.WriteLine("\n--- 6. Отмена длительных операций ---");

        // CancellationTokenSource - источник токенов отмены
        // CancellationToken - сам токен, который передается в задачу
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Создаем задачу, которая периодически проверяет токен отмены
        Task cancelableTask = Task.Run(() =>
        {
            Console.WriteLine("Отменяемая задача начала работу");

            for (int i = 0; i < 10; i++)
            {
                // Проверяем, не запрошена ли отмена
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Обнаружен запрос отмены, завершаю работу...");
                    // Генерируем специальное исключение отмены
                    cancellationToken.ThrowIfCancellationRequested();
                }

                Console.WriteLine($"  Выполняется итерация {i}");
                Thread.Sleep(500); // Имитация работы
            }

            Console.WriteLine("Задача завершилась нормально (отмена не запрашивалась)");
        }, cancellationToken); // Передаем токен в задачу

        // Запускаем отдельную задачу для отмены
        Task cancellationTrigger = Task.Run(() =>
        {
            Thread.Sleep(2000); // Ждем 2 секунды
            Console.WriteLine("=== АКТИВИРУЮ ОТМЕНУ ===");
            cancellationTokenSource.Cancel(); // Запрашиваем отмену
        });

        try
        {
            // Ждем завершения основной задачи
            cancelableTask.Wait();
            Console.WriteLine("Задача завершилась без отмены");
        }
        catch (AggregateException ae)
        {
            // Обрабатываем возможные исключения
            if (ae.InnerExceptions[0] is TaskCanceledException)
            {
                Console.WriteLine("✓ Задача была корректно отменена через TaskCanceledException");
            }
            else
            {
                Console.WriteLine($"✗ Задача упала с другой ошибкой: {ae.InnerExceptions[0].Message}");
            }
        }

        // Проверяем финальный статус задачи
        Console.WriteLine($"Финальный статус задачи: {cancelableTask.Status}");
    }
}