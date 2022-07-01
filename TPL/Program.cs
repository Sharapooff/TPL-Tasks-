using System.Threading.Tasks;

Console.WriteLine("AppRun!");
//3 варианта запуска--------------------------------------------------------------------------------------------------------------------------------
Task task_1 = new Task(() => Console.WriteLine("Task_1 Run")); task_1.Start();
Task task_2 = Task.Factory.StartNew(() => Console.WriteLine("Task_2 Run"));
Task task_3 = Task.Run(() => Console.WriteLine("Task_3 Run"));
//Ожидание завершения задачи (метод Wait() объекта Task)
task_1.Wait();   // ожидаем завершения задачи task1
task_2.Wait();   // ожидаем завершения задачи task2
task_3.Wait();   // ожидаем завершения задачи task3
                 //В этом случае приложение все равно будет ждать завершения задачи,
                 //однако другие синхронные действия в основном потоке не будут блокироваться и ожидать завершения задачи.

//Синхронный запуск задачи---------------------------------------------------------------------------------------------------------------------------
//По умолчанию задачи запускаются асинхронно. Однако с помощью метода RunSynchronously() можно запускать синхронно:
Console.WriteLine("Main Starts_______");
// создаем задачу
Task task1 = new Task(() =>
{
    Console.WriteLine("Task Starts");
    Thread.Sleep(1000);
    Console.WriteLine("Task Ends");
});
task1.RunSynchronously(); // запускаем задачу синхронно
Console.WriteLine("Main Ends________"); // этот вызов ждет завершения задачи task1

//Вложенные задачи (Одна задача может запускать другую - вложенную задачу)----------------------------------------------------------------------------
//вложенная задача может завершить выполнение даже после завершения метода Main
var outer = Task.Factory.StartNew(() =>      // внешняя задача
{
    Console.WriteLine("Outer task starting...");
    var inner = Task.Factory.StartNew(() =>  // вложенная задача
    {
        Console.WriteLine("Inner task starting...");
        Thread.Sleep(2000);
        Console.WriteLine("Inner task finished.");
    });
});
outer.Wait(); // ожидаем выполнения внешней задачи
Console.WriteLine("End of Main______");
//Если необходимо, чтобы вложенная задача выполнялась как часть внешней,--------------------------------------------------------------------------------
//необходимо использовать значение TaskCreationOptions.AttachedToParent:
Console.WriteLine("Main Starts_______");
outer = Task.Factory.StartNew(() =>      // внешняя задача
{
    Console.WriteLine("Outer task starting...");
    var inner = Task.Factory.StartNew(() =>  // вложенная задача
    {
        Console.WriteLine("Inner task starting...");
        Thread.Sleep(2000);
        Console.WriteLine("Inner task finished.");
    }, TaskCreationOptions.AttachedToParent);
});
outer.Wait(); // ожидаем выполнения внешней задачи
Console.WriteLine("End of Main______");

//Массив задач__________________________________________________________________________________________________________________________________________
Console.WriteLine("Массив задач______");
Task[] tasks1 = new Task[3]
{
    new Task(() => Console.WriteLine("First Task")),
    new Task(() => Console.WriteLine("Second Task")),
    new Task(() => Console.WriteLine("Third Task"))
};
// запуск задач в массиве
foreach (var t in tasks1)
    t.Start();
/* //либо так
    Task[] tasks2 = new Task[3];
    int j = 1;
    for (int i = 0; i < tasks2.Length; i++)
        tasks2[i] = Task.Factory.StartNew(() => Console.WriteLine($"Task {j++}"));
*/
//Если необходимо завершить выполнение программы или вообще выполнять некоторый код лишь после того,
//как все задачи из массива завершатся, то применяется метод Task.WaitAll(tasks):
Console.WriteLine("Массив задач______");
Task[] tasks = new Task[3];
for (var i = 0; i < tasks.Length; i++)
{
    tasks[i] = new Task(() =>
    {
        Thread.Sleep(1000); // эмуляция долгой работы
        Console.WriteLine($"Task{i} finished");
    });
    tasks[i].Start();   // запускаем задачу
}
Task.WaitAll(tasks); // ожидаем завершения всех задач
Console.WriteLine("Завершение метода Main_____________");

//Возвращение результатов из задач----------------------------------------------------------------------------------------------------------------------
//При этом при обращении к свойству Result текущий поток останавливает выполнение и ждет, когда будет получен результат из выполняемой задачи.
int n1 = 4, n2 = 5;
Task<int> sumTask = new Task<int>(() => Sum(n1, n2));
sumTask.Start();
int result = sumTask.Result;
Console.WriteLine($"{n1}+{n2}={result}");
int Sum(int a, int b) => a + b;
//Задачи продолжения_________________________________________________________________________________________________________________________
Console.WriteLine("Задачи продолжения_____________");
task1 = new Task(() =>
{
    Console.WriteLine($"Id задачи: {Task.CurrentId}");
});
// задача продолжения - task2 выполняется после task1
Task task2 = task1.ContinueWith(PrintTask);
task1.Start();
// ждем окончания второй задачи
task2.Wait();
Console.WriteLine("Конец метода Main");
void PrintTask(Task t)
{
    Console.WriteLine($"Id задачи: {Task.CurrentId}");
    Console.WriteLine($"Id предыдущей задачи: {t.Id}");
    Thread.Sleep(3000);
}
//
sumTask = new Task<int>(() => Sum(4, 5));
// задача продолжения
Task printTask = sumTask.ContinueWith(task => PrintResult(task.Result));
sumTask.Start();
// ждем окончания второй задачи
printTask.Wait();
Console.WriteLine("Конец метода Main________");
void PrintResult(int sum) => Console.WriteLine($"Sum: {sum}");

//Класс Parallel______________________________________________________________________________________________________________
//Метод Parallel.Invoke в качестве параметра принимает массив объектов Action,
//то есть мы можем передать в данный метод набор методов, которые будут вызываться при его выполнении
// метод Parallel.Invoke выполняет три метода
Console.WriteLine("Класс Parallel________");
Parallel.Invoke(
    Print,
    () =>
    {
        Console.WriteLine($"Выполняется задача {Task.CurrentId}");
        Thread.Sleep(3000);
    },
    () => Square(5)
);

void Print()
{
    Console.WriteLine($"Выполняется задача {Task.CurrentId}");
    Thread.Sleep(3000);
}
// вычисляем квадрат числа
void Square(int n)
{
    Console.WriteLine($"Выполняется задача {Task.CurrentId}");
    Thread.Sleep(3000);
    Console.WriteLine($"Результат {n * n}");
}
//Parallel.For_______________________________________________________________________________________________________________
//For(int, int, Action<int>)
Console.WriteLine("Parallel.For________");
Parallel.For(10, 11, Square);

//Отмена задач и параллельных операций. CancellationToken____________________________________________________________________
//Мягкий выход из задачи без исключения
CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
CancellationToken token = cancelTokenSource.Token;
Console.WriteLine("CancellationToken________");
// задача вычисляет квадраты чисел
Task task = new Task(() =>
{
    for (int i = 1; i < 10; i++)
    {
        if (token.IsCancellationRequested)  // проверяем наличие сигнала отмены задачи
        {
            Console.WriteLine("Операция прервана");
            return;     //  выходим из метода и тем самым завершаем задачу
        }
        Console.WriteLine($"Квадрат числа {i} равен {i * i}");
        Thread.Sleep(200);
    }
}, token);
task.Start();
Thread.Sleep(1000);// после задержки по времени отменяем выполнение задачи
cancelTokenSource.Cancel();// ожидаем завершения задачи
Thread.Sleep(1000);//  проверяем статус задачи
Console.WriteLine($"Task Status: {task.Status}");
cancelTokenSource.Dispose(); // освобождаем ресурсы

//Второй способ завершения задачи представляет генерация исключения OperationCanceledException.
//Для этого применяется метод ThrowIfCancellationRequested() объекта CancellationToken
//Исключение возникает только тогда, когда мы останавливаем текущий поток и ожидаем завершения задачи с помощью методов Wait или WaitAll
//CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
//CancellationToken token = cancelTokenSource.Token;

//Task task = new Task(() =>
//{
//    for (int i = 1; i < 10; i++)
//    {
//        if (token.IsCancellationRequested)
//            token.ThrowIfCancellationRequested(); // генерируем исключение

//        Console.WriteLine($"Квадрат числа {i} равен {i * i}");
//        Thread.Sleep(200);
//    }
//}, token);
//try
//{
//    task.Start();
//    Thread.Sleep(1000);
//    // после задержки по времени отменяем выполнение задачи
//    cancelTokenSource.Cancel();

//    task.Wait(); // ожидаем завершения задачи
//}
//catch (AggregateException ae)
//{
//    foreach (Exception e in ae.InnerExceptions)
//    {
//        if (e is TaskCanceledException)
//            Console.WriteLine("Операция прервана");
//        else
//            Console.WriteLine(e.Message);
//    }
//}
//finally
//{
//    cancelTokenSource.Dispose();
//}

////  проверяем статус задачи
//Console.WriteLine($"Task Status: {task.Status}");

//Регистрация обработчика отмены задачи (Метод Register() позволяет зарегистрировать обработчик отмены задачи в виде делегата Action)
//CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
//CancellationToken token = cancelTokenSource.Token;

//// задача вычисляет квадраты чисел
//Task task = new Task(() =>
//{
//    int i = 1;
//    token.Register(() =>
//    {
//        Console.WriteLine("Операция прервана");
//        i = 10;
//    });
//    for (; i < 10; i++)
//    {
//        Console.WriteLine($"Квадрат числа {i} равен {i * i}");
//        Thread.Sleep(400);
//    }
//}, token);
//task.Start();

//Thread.Sleep(1000);
//// после задержки по времени отменяем выполнение задачи
//cancelTokenSource.Cancel();
//// ожидаем завершения задачи
//Thread.Sleep(1000);
////  проверяем статус задачи
//Console.WriteLine($"Task Status: {task.Status}");
//cancelTokenSource.Dispose(); // освобождаем ресурсы
