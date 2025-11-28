
/*
namespace ConsoleApp21
{

    public static ParallelLoopResult ForEach<TSource>(
             IEnumerable<TSource> source,
             Action<TSource> body)

    public static void GrayscaleTransformation(IEnumerable<Frame> Movie)
    {
        var ProcessedMovie =
        Movie
        .AsParallel()
        .AsOrdered()
        .Select(frame => ConvertToGrayscale(frame));
        foreach (var grayscaleFrame in ProcessedMovie)
        {
            // Movie frames will be evaluated lazily
        }

    public static ParallelLoopResult ForEach<TSource>(
                IEnumerable<TSource> source,
                Action<TSource, ParallelLoopState, Int64> body)

    public static double[] PairwiseMultiply(double[] v1, double[] v2)
    {
        var length = Math.Min(v1.Length, v2.Lenth);
        double[] result = new double[length];
        Parallel.ForEach(v1,
          (element, loopstate, elementIndex) =>
          result[elementIndex] = element * v2[elementIndex]);
        return result;
    }
    /*
     * PLINQ предлагает возможность обработки запроса как запроса над потоком. Эта возможность крайне ценна по следующим причинам:
     1. Результаты не материализуются в массиве, таким образом, нет избыточности хранения данных в памяти.
     2. Вы можете получать (enumerate) результаты в одиночный поток вычислений по мере получения новых данных.
     
    public static void AnalyzeStocks(IEnumerable<Stock> Stocks)
    {
        var StockRiskPortfolio =
        Stocks
        .AsParallel()
        .AsOrdered()
        .Select(stock => new { Stock = stock, Risk = ComputeRisk(stock) })
        .Where(stockRisk => ExpensiveRiskAnalysis(stockRisk.Risk));
        foreach (var stockRisk in StockRiskPortfolio)
        {
            SomeStockComputation(stockRisk.Risk);
            // StockRiskPortfolio will be a stream of results
        }
    }
    /*
     * ParallelMergeOptions.NotBuffered — указывает, что каждый обработанный элемент возвращается из каждого потока, как только он обработан
       ParallelMergeOptions.AutoBuffered — указывает, что элементы собираются в буфер, буфера периодически возвращается потоку-потребителю
       ParallelMergeOptions.FullyBuffered — указывает, что выходная последовательность полностью буферизуется, это позволяет получить результаты быстрее, чем при использовании других вариантов, однако тогда потоку-потребителю придется долго дожидаться получения первого элемента для обработки.
    


    public static IEnumerable<T> Zipping<T>(IEnumerable<T> a, IEnumerable<T> b)
    {
        return
        a
        .AsParallel()
        .AsOrdered()
        .Select(element => ExpensiveComputation(element))
        .Zip(
          b
          .AsParallel()
          .AsOrdered()
          .Select(element => DifferentExpensiveComputation(element)),
          (a_element, b_element) => Combine(a_element, b_element));
    }

огда использовать PLINQ:
✅ Большие коллекции (тысячи+ элементов)

✅ CPU-интенсивные операции над элементами

✅ Независимая обработка элементов

✅ Многопроцессорные системы

Когда НЕ использовать PLINQ:
❌ Маленькие коллекции (< 1000 элементов)

❌ Простые операции (накладные расходы > выгоды)

❌ Операции с побочными эффектами

❌ Когда важен строгий порядок элементов

Оптимизации PLINQ:
🔧 AsOrdered() - когда важен порядок

🔧 WithDegreeOfParallelism() - ограничение потоков

🔧 WithExecutionMode() - принудительный параллелизм

🔧 WithMergeOptions() - стратегии слияния

🔧 WithCancellation() - отмена длительных операций
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class PlinqExamples
{
    static void Main()
    {
        Console.WriteLine("=== PLINQ (Parallel LINQ) - Параллельные запросы к данным ===");

        // PLINQ - это параллельная реализация LINQ to Objects
        // Автоматически распределяет обработку элементов коллекции по нескольким потокам

        BasicPlinqExamples();           // Основы PLINQ запросов
        OrderPreservationExample();     // Сохранение порядка элементов
        PerformanceComparison();        // Сравнение с обычным LINQ
        ExceptionHandlingExample();     // Обработка ошибок
        CancellationExample();          // Отмена длительных запросов
        SpecialOperationsExample();     // Специальные возможности PLINQ

        Console.ReadLine();
    }

    static void BasicPlinqExamples()
    {
        Console.WriteLine("\n--- 1. Базовые примеры PLINQ ---");

        var numbers = Enumerable.Range(1, 20);
        Console.WriteLine($"Исходная коллекция: [{string.Join(", ", numbers)}]");

        // Самый простой PLINQ запрос: добавляем .AsParallel() к любой коллекции
        // Это превращает обычный LINQ запрос в параллельный
        var parallelQuery = numbers
            .AsParallel()  // ★ Волшебное преобразование в параллельный запрос ★
            .Where(x =>
            {
                Console.WriteLine($"  Проверка числа {x} в потоке {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(100); // Имитация сложной проверки
                return x % 2 == 0; // Ищем четные числа
            })
            .Select(x =>
            {
                Console.WriteLine($"  Возведение в квадрат {x} в потоке {Thread.CurrentThread.ManagedThreadId}");
                return x * x; // Квадрат числа
            });

        // Запрос выполняется только когда мы начинаем перечислять результаты
        Console.WriteLine("Четные числа в квадрате (параллельно):");
        foreach (var result in parallelQuery)
        {
            Console.WriteLine($"  Результат: {result}");
        }

        // PLINQ с агрегатными операциями (Sum, Average, Count и т.д.)
        Console.WriteLine("\n--- Агрегатные операции в PLINQ ---");
        var sum = numbers
            .AsParallel()
            .Where(x => x > 10) // Фильтруем числа больше 10
            .Sum();             // Суммируем - операция автоматически параллелится

        Console.WriteLine($"Сумма чисел > 10: {sum}");

        // PLINQ с пользовательской функцией обработки
        Console.WriteLine("\n--- PLINQ с пользовательской логикой ---");
        var complexResult = numbers
            .AsParallel()
            .Where(x =>
            {
                // Сложная проверка - каждая выполняется в отдельном потоке
                return IsPrime(x);
            })
            .Select(x => $"Простое число: {x}")
            .ToArray(); // Принудительное выполнение запроса и материализация результатов

        Console.WriteLine($"Найдено простых чисел: {complexResult.Length}");
        Console.WriteLine($"Первые 5: {string.Join(", ", complexResult.Take(5))}");
    }

    static void OrderPreservationExample()
    {
        Console.WriteLine("\n--- 2. Сохранение порядка элементов в PLINQ ---");

        var numbers = Enumerable.Range(1, 15);
        Console.WriteLine($"Исходная коллекция: [{string.Join(", ", numbers)}]");

        // ★ ВАЖНО: По умолчанию PLINQ НЕ сохраняет порядок элементов для увеличения производительности ★
        var unordered = numbers
            .AsParallel()
            .AsUnordered() // Явное указание - порядок не важен (используется по умолчанию)
            .Where(x => x % 3 == 0)
            .Select(x =>
            {
                // Имитируем разное время обработки для разных элементов
                Thread.Sleep(x * 10);
                return x;
            });

        Console.WriteLine($"Без сохранения порядка: [{string.Join(", ", unordered)}]");
        Console.WriteLine("  Элементы могут быть в произвольном порядке!");

        // ★ Сохранение порядка с помощью .AsOrdered() ★
        var ordered = numbers
            .AsParallel()
            .AsOrdered() // ★ Гарантируем сохранение исходного порядка ★
            .Where(x => x % 3 == 0)
            .Select(x =>
            {
                Thread.Sleep(x * 10); // Разное время обработки
                return x;
            });

        Console.WriteLine($"С сохранением порядка: [{string.Join(", ", ordered)}]");
        Console.WriteLine("  Порядок элементов соответствует исходной коллекции");

        // Когда порядок критически важен:
        var orderedTake = numbers
            .AsParallel()
            .AsOrdered() // Без AsOrdered() Take(5) может взять любые 5 элементов
            .Where(x => x % 2 == 0)
            .Take(5)     // Нужны именно ПЕРВЫЕ 5 четных чисел в исходном порядке
            .Select(x => x);

        Console.WriteLine($"Первые 5 четных чисел: [{string.Join(", ", orderedTake)}]");

        // ★ Сравнение производительности ★
        Console.WriteLine("\n--- Сравнение скорости Ordered vs Unordered ---");
        var largeData = Enumerable.Range(1, 10000);

        var sw = Stopwatch.StartNew();
        var unorderedCount = largeData.AsParallel().AsUnordered().Where(x => x % 7 == 0).Count();
        sw.Stop();
        var unorderedTime = sw.ElapsedMilliseconds;

        sw.Restart();
        var orderedCount = largeData.AsParallel().AsOrdered().Where(x => x % 7 == 0).Count();
        sw.Stop();
        var orderedTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"Unordered: {unorderedTime}ms, Ordered: {orderedTime}ms");
        Console.WriteLine($"Разница: {orderedTime - unorderedTime}ms (Unordered быстрее!)");
    }

    static void PerformanceComparison()
    {
        Console.WriteLine("\n--- 3. Сравнение производительности LINQ vs PLINQ ---");

        // ★ ВАЖНО: PLINQ не всегда быстрее! Накладные расходы на многопоточность могут перевесить выгоду ★

        var largeData = Enumerable.Range(1, 1000000);
        Console.WriteLine($"Тест на коллекции из {largeData.Count()} элементов");

        // Тест 1: Простая операция (фильтрация + преобразование)
        Console.WriteLine("\n--- Тест 1: Простая операция ---");

        var sw = Stopwatch.StartNew();
        var linqResult = largeData
            .Where(x => x % 2 == 0)          // Фильтрация
            .eleSct(x => Math.Sqrt(x))       // Математическое преобразование
            .Sum();                          // Агрегация
        sw.Stop();
        var linqTime = sw.ElapsedMilliseconds;

        sw.Restart();
        var plinqResult = largeData
            .AsParallel()                    // ★ Включаем параллелизм ★
            .Where(x => x % 2 == 0)
            .Select(x => Math.Sqrt(x))
            .Sum();
        sw.Stop();
        var plinqTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"LINQ:  результат = {linqResult:F2}, время = {linqTime}ms");
        Console.WriteLine($"PLINQ: результат = {plinqResult:F2}, время = {plinqTime}ms");
        Console.WriteLine($"Ускорение: {(double)linqTime / plinqTime:F2}x");

        // Тест 2: CPU-интенсивная операция
        Console.WriteLine("\n--- Тест 2: CPU-интенсивная операция ---");

        sw.Restart();
        var linqPrimes = largeData
            .Where(x => IsPrime(x))  // Проверка на простоту - тяжелая операция
            .Count();
        sw.Stop();
        linqTime = sw.ElapsedMilliseconds;

        sw.Restart();
        var plinqPrimes = largeData
            .AsParallel()
            .Where(x => IsPrime(x))  // ★ Здесь PLINQ должен показать преимущество ★
            .Count();
        sw.Stop();
        plinqTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"LINQ:  найдено {linqPrimes} простых чисел, время = {linqTime}ms");
        Console.WriteLine($"PLINQ: найдено {plinqPrimes} простых чисел, время = {plinqTime}ms");
        Console.WriteLine($"Ускорение: {(double)linqTime / plinqTime:F2}x");

        // ★ Влияние степени параллелизма на производительность ★
        Console.WriteLine("\n--- Влияние степени параллелизма ---");
        Console.WriteLine("Тестируем разное количество потоков для CPU-интенсивной операции:");

        for (int degree = 1; degree <= Environment.ProcessorCount; degree++)
        {
            sw.Restart();
            var result = largeData
                .AsParallel()
                .WithDegreeOfParallelism(degree) // ★ Ограничиваем количество потоков ★
                .Where(x => IsPrime(x))
                .Count();
            sw.Stop();

            Console.WriteLine($"  Степень параллелизма {degree}: {result} простых чисел, время: {sw.ElapsedMilliseconds}ms");
        }

        Console.WriteLine($"\n★ Процессор имеет {Environment.ProcessorCount} ядер ★");
        Console.WriteLine("Оптимальная степень параллелизма обычно равна количеству ядер");
    }

    static void ExceptionHandlingExample()
    {
        Console.WriteLine("\n--- 4. Обработка исключений в PLINQ ---");

        // ★ ВАЖНО: Исключения в PLINQ оборачиваются в AggregateException ★

        var numbers = Enumerable.Range(-5, 15); // Коллекция содержит отрицательные числа
        Console.WriteLine($"Тестовая коллекция: [{string.Join(", ", numbers)}]");

        // Пример 1: Исключение в Select
        try
        {
            var result = numbers
                .AsParallel()
                .Select(x =>
                {
                    if (x < 0)
                    {
                        // Генерируем исключение для отрицательных чисел
                        throw new ArgumentException($"Отрицательное число обнаружено: {x}");
                    }
                    return Math.Sqrt(x); // Квадратный корень
                })
                .ToArray(); // ★ Исключения проявятся при выполнении запроса ★
        }
        catch (AggregateException ae)
        {
            Console.WriteLine("Поймано AggregateException с внутренними исключениями:");
            foreach (var e in ae.InnerExceptions)
            {
                Console.WriteLine($"  → {e.GetType().Name}: {e.Message}");
            }
        }

        // Пример 2: Обработка исключений с помощью обработчиков
        Console.WriteLine("\n--- Обработка исключений с помощью Handle ---");

        try
        {
            var query = from num in numbers.AsParallel()
                        where num > 0
                        select 100 / num; // ★ Возможное деление на ноль ★

            var results = query.ToArray();
        }
        catch (AggregateException ae)
        {
            // Handle позволяет фильтровать исключения
            ae.Handle(e =>
            {
                if (e is DivideByZeroException)
                {
                    Console.WriteLine("✓ Обработано деление на ноль - продолжить выполнение");
                    return true; // Исключение обработано, система продолжит работу
                }

                // Все другие исключения не обрабатываем
                Console.WriteLine("✗ Необработанное исключение другого типа");
                return false; // Исключение не обработано, будет выброшено дальше
            });
        }

        // Пример 3: Безопасный запрос с отловом всех исключений
        Console.WriteLine("\n--- Безопасный PLINQ запрос ---");

        var safeQuery = numbers
            .AsParallel()
            .Where(x =>
            {
                try
                {
                    return x > 0 && IsPrime(x);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"  Исключение обработано внутри: {e.Message}");
                    return false;
                }
            })
            .Select(x => x);

        Console.WriteLine($"Безопасные результаты: [{string.Join(", ", safeQuery)}]");
    }

    static void CancellationExample()
    {
        Console.WriteLine("\n--- 5. Отмена PLINQ запросов ---");

        // ★ Механизм отмены через CancellationTokenSource ★

        var cancellationSource = new CancellationTokenSource();
        var token = cancellationSource.Token;

        var largeData = Enumerable.Range(1, 1000000);

        Console.WriteLine("Запускаю длительный PLINQ запрос...");

        // Задача, которая отменит запрос через 100ms
        Task cancellationTask = Task.Run(() =>
        {
            Thread.Sleep(100); // Даем запросу немного поработать
            Console.WriteLine("=== ЗАПРАШИВАЮ ОТМЕНУ PLINQ ЗАПРОСА ===");
            cancellationSource.Cancel();
        });

        try
        {
            var result = largeData
                .AsParallel()
                .WithCancellation(token) // ★ Передаем токен отмены в PLINQ ★
                .Where(x =>
                {
                    // Имитация тяжелой CPU-работы
                    for (int i = 0; i < 100; i++)
                    {
                        // Периодически проверяем не запрошена ли отмена
                        if (token.IsCancellationRequested)
                        {
                            Console.WriteLine($"  Обнаружена отмена на элементе {x}");
                            token.ThrowIfCancellationRequested();
                        }
                    }
                    return x % 2 == 0;
                })
                .Select(x => x)
                .Count(); // Выполняем запрос

            Console.WriteLine($"Запрос завершился нормально. Результат: {result}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("✓ PLINQ запрос был корректно отменен через OperationCanceledException");
        }
        catch (AggregateException ae) when (ae.InnerExceptions.Any(e => e is OperationCanceledException))
        {
            Console.WriteLine("✓ PLINQ запрос отменен (через AggregateException)");
        }

        // ★ Автоматическая проверка отмены в PLINQ ★
        Console.WriteLine("\n--- Автоматическая проверка отмены ---");

        var cts2 = new CancellationTokenSource();
        cts2.CancelAfter(50); // Автоматическая отмена через 50ms

        try
        {
            var result2 = largeData
                .AsParallel()
                .WithCancellation(cts2.Token)
                .Where(x =>
                {
                    // Даже без явной проверки токена, PLINQ периодически проверяет отмену
                    Thread.Sleep(1); // Небольшая задержка
                    return x % 3 == 0;
                })
                .Count();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("✓ PLINQ автоматически обнаружил отмену без явных проверок в коде");
        }
    }

    static void SpecialOperationsExample()
    {
        Console.WriteLine("\n--- 6. Специальные операции и настройки PLINQ ---");

        var numbers = Enumerable.Range(1, 100);

        // ★ ForAll - параллельное выполнение действия для всех элементов ★
        Console.WriteLine("--- ForAll: Параллельное выполнение действий ---");
        Console.WriteLine("Обрабатываем кратные 10 числа:");

        numbers
            .AsParallel()
            .Where(x => x % 10 == 0)
            .ForAll(x =>
            {
                // Это действие выполняется параллельно для КАЖДОГО элемента
                // В отличие от foreach, который выполняется последовательно
                Console.WriteLine($"  Обработка {x} в потоке {Thread.CurrentThread.ManagedThreadId}");
            });

        // ★ AsSequential - временное переключение на последовательное выполнение ★
        Console.WriteLine("\n--- AsSequential: Смешанный параллельно-последовательный запрос ---");

        var mixedQuery = numbers
            .AsParallel()                   // Начало - параллельное выполнение
            .Where(x => x % 2 == 0)        // Фильтрация - параллельно
            .AsSequential()                // ★ Переключаемся на последовательное выполнение ★
            .Where(x => x % 4 == 0)        // Фильтрация - последовательно (сохраняется порядок)
            .AsParallel()                  // Снова переключаемся на параллельное выполнение
            .Select(x => x * x);           // Преобразование - параллельно

        Console.WriteLine($"Смешанный запрос (первые 10): {string.Join(", ", mixedQuery.Take(10))}");

        // ★ WithExecutionMode - принудительное параллельное выполнение ★
        Console.WriteLine("\n--- WithExecutionMode: Принудительный параллелизм ---");

        var smallData = Enumerable.Range(1, 50); // Маленькая коллекция

        // PLINQ может решить, что для маленьких коллекций параллелизм не эффективен (10000)
        // WithExecutionMode.ForceParallelism заставляет использовать параллелизм в любом случае
        var forcedParallel = smallData
            .AsParallel()
            .WithExecutionMode(ParallelExecutionMode.ForceParallelism) // ★ Принудительно ★
            .Where(x => x % 5 == 0)
            .ToArray();

        Console.WriteLine($"Принудительный параллелизм: {string.Join(", ", forcedParallel)}");

        // ★ WithMergeOptions - настройка слияния результатов из разных потоков ★
        Console.WriteLine("\n--- WithMergeOptions: Стратегии слияния результатов ---");

        Console.WriteLine("NotBuffered - минимальная задержка, но больше накладных расходов:");
        var notBufferedQuery = numbers
            .AsParallel()
            .WithMergeOptions(ParallelMergeOptions.NotBuffered)
            .Where(x => x % 7 == 0)
            .Select(x =>
            {
                Thread.Sleep(50); // Имитация обработки
                return x;
            });

        // Результаты начинают поступать сразу как готовы
        foreach (var item in notBufferedQuery)
        {
            Console.WriteLine($"  Получен результат: {item}");
        }

        Console.WriteLine("\nFullyBuffered - максимальная производительность, но задержка перед первым результатом:");
        var bufferedQuery = numbers
            .AsParallel()
            .WithMergeOptions(ParallelMergeOptions.FullyBuffered) // ★ Буферизация всех результатов ★
            .Where(x => x % 7 == 0)
            .Select(x =>
            {
                Thread.Sleep(50);
                return x;
            });

        // Все результаты буферизуются, затем выдаютсся сразу
        foreach (var item in bufferedQuery)
        {
            Console.WriteLine($"  Получен результат: {item}");
        }
    }

    // ★ Вспомогательный метод для проверки простых чисел ★
    static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        // Проверяем делители до квадратного корня из числа
        var boundary = (int)Math.Floor(Math.Sqrt(number));

        for (int i = 3; i <= boundary; i += 2)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }
}
