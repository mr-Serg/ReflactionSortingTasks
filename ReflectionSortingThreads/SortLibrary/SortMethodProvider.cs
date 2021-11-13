using System;
using System.ComponentModel;
using System.Threading;

namespace SortingLibrary
{
    // The special attribute marks sorting methods designed for a binding at run time.
    // It holds the method name for the user interface
    // * Спеціальний атрибут позначає методи сортування, призначені для пізнього зв'язування
    // * Містить назву методу для використання в інтерфейсі користувача
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MethodNameAttribute: System.Attribute
    {
        public string OuterName { get; set; }
        public string LocalName { get; set; }
        public MethodNameAttribute(string name)
        {
            OuterName = name;
        }
    }

    // Призначення атрибуту таке саме: виділити методи і дати їм імена, проте 
    // це ті методи, які можна вкладати в Task
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TaskNameAttribute : System.Attribute
    {
        public string OuterName { get; set; }
        public string LocalName { get; set; }
        public TaskNameAttribute(string name)
        {
            OuterName = name;
        }
    }

    // The class contains different methods for sorting in nondecreasing order an array of
    // integers: usual ones and methods adapted for interaction with a BackgroundWorker.
    // * Клас містить різноманітні методи впорядкування масиву цілих чисел (за зростанням):
    // * звичані та пристосовані до взаємодії з екземпляром BackgroundWorker [MethoName]
    // * а також пристосовані для запуску в Task - позначені [TaskName]
    public static class SortMethodProvider
    {
        private const int delay = 15;
        private const int partOfDelay = delay / 5;

        // Bubble sort algorithm (simple exchange method)
        // * впорядкування масиву цілих за зростанням методом обміну ("бульбашки")
        [MethodNameAttribute("Bubble sort", LocalName = "Метод бульбашки")]
        public static void BubbleSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            for (int i = arrayToSort.Length - 1; i > 0; --i)
            {
                for (int j = 0; j < i; ++j)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    // Looking for pairs of neighboring elements situated in wrong order
                    // * шукаємо "неправильні" пари сусідів
                    if (arrayToSort[j] > arrayToSort[j + 1])
                    {
                        // The exchange: to correct a "wrong pair" of elements
                        // and the exchange visualization in the main thread
                        // * власне обмін - "виправляємо" пару сусідів
                        // * відображення обміну в головному потоці
                        int t = arrayToSort[j];
                        worker.ReportProgress(257 * 1000 + arrayToSort[j]);
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j] = arrayToSort[j + 1];
                        worker.ReportProgress(j * 1000 + arrayToSort[j + 1]);
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j + 1] = t;
                        worker.ReportProgress((j + 1) * 1000 + t);
                        System.Threading.Thread.Sleep(partOfDelay);
                    }
                }
            }
        }

        [TaskNameAttribute("Bubble sort", LocalName = "Метод бульбашки")]
        public static void BubbleSortInTask(int[] arrayToSort,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            for (int i = arrayToSort.Length - 1; i > 0; --i)
            {
                for (int j = 0; j < i; ++j)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    // Looking for pairs of neighboring elements situated in wrong order
                    // * шукаємо "неправильні" пари сусідів
                    if (arrayToSort[j] > arrayToSort[j + 1])
                    {
                        // The exchange: to correct a "wrong pair" of elements
                        // and the exchange visualization in the main thread
                        // * власне обмін - "виправляємо" пару сусідів
                        // * відображення обміну в головному потоці
                        int t = arrayToSort[j];
                        progress.Report((257, arrayToSort[j]));
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j] = arrayToSort[j + 1];
                        progress.Report((j, arrayToSort[j + 1]));
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j + 1] = t;
                        progress.Report((j + 1, t));
                        System.Threading.Thread.Sleep(partOfDelay);
                    }
                }
            }
        }

        // Improved exchande sort algorithm compares "gap-neighboring" elements
        // * метод гребінця порівнює і обмінює пари gap-віддалених елементів
        [MethodNameAttribute("Comb sort", LocalName = "Метод \"гребінця\"")]
        public static void ComboSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            double shrink = 1.3;
            int gap = arrayToSort.Length;
            bool swapped = true;

            while (gap > 1 || swapped)
            {
                gap = (int)(gap / shrink);
                if (gap < 1) gap = 1;
                swapped = false;

                for (int j = 0; j < arrayToSort.Length - gap; ++j)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    // Looking for pairs of "gap-neighboring" elements situated in wrong order
                    // * шукаємо "неправильні" пари елементів на відстані gap один від одного
                    if (arrayToSort[j] > arrayToSort[j + gap])
                    {
                        // The exchange: to correct a "wrong pair" of elements
                        // and the exchange visualization in the main thread
                        // * власне обмін - "виправляємо" знайдену пару елементів
                        // * відображення обміну в головному потоці
                        int t = arrayToSort[j];
                        worker.ReportProgress(257 * 1000 + arrayToSort[j]);
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j] = arrayToSort[j + gap];
                        worker.ReportProgress(j * 1000 + arrayToSort[j + gap]);
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j + gap] = t;
                        worker.ReportProgress((j + gap) * 1000 + t);
                        System.Threading.Thread.Sleep(partOfDelay);

                        swapped = true;
                    }
                }
            }
        }

        [TaskNameAttribute("Comb sort", LocalName = "Метод \"гребінця\"")]
        public static void ComboSortInTask(int[] arrayToSort,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            double shrink = 1.3;
            int gap = arrayToSort.Length;
            bool swapped = true;

            while (gap > 1 || swapped)
            {
                gap = (int)(gap / shrink);
                if (gap < 1) gap = 1;
                swapped = false;

                for (int j = 0; j < arrayToSort.Length - gap; ++j)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    // Looking for pairs of "gap-neighboring" elements situated in wrong order
                    // * шукаємо "неправильні" пари елементів на відстані gap один від одного
                    if (arrayToSort[j] > arrayToSort[j + gap])
                    {
                        // The exchange: to correct a "wrong pair" of elements
                        // and the exchange visualization in the main thread
                        // * власне обмін - "виправляємо" знайдену пару елементів
                        // * відображення обміну в головному потоці
                        int t = arrayToSort[j];
                        progress.Report((257, arrayToSort[j]));
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j] = arrayToSort[j + gap];
                        progress.Report((j, arrayToSort[j + gap]));
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j + gap] = t;
                        progress.Report((j + gap, t));
                        System.Threading.Thread.Sleep(partOfDelay);

                        swapped = true;
                    }
                }
            }
        }

        // Improved exchange sort algorithm swithes direction of the elements looking through
        // * метод гнома прудко доставляє малі елементи на початок, великі - в кінець
        [MethodNameAttribute("\"Gnome sort\"", LocalName = "Метод \"гнома\"")]
        public static void GnomeSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            for (int i = 0; i < arrayToSort.Length; )
            {
                // Cancel request checking
                // * перевірка на потребу зупинити потік
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                // Looking for pairs of neighboring elements situated in wrong order
                // * шукаємо "неправильні" пари сусідів
                if (i == 0 || arrayToSort[i - 1] <= arrayToSort[i])
                {
                    ++i;
                }
                else
                {
                    // The exchange: to correct a "wrong pair" of elements
                    // and the exchange visualization in the main thread
                    // * власне обмін - "виправляємо" знайдену пару елементів
                    // * відображення обміну в головному потоці
                    int t = arrayToSort[i];
                    worker.ReportProgress(257 * 1000 + arrayToSort[i]);
                    System.Threading.Thread.Sleep(partOfDelay);

                    arrayToSort[i] = arrayToSort[i - 1];
                    worker.ReportProgress(i * 1000 + arrayToSort[i - 1]);
                    System.Threading.Thread.Sleep(partOfDelay);

                    arrayToSort[i - 1] = t;
                    worker.ReportProgress((i - 1) * 1000 + t);
                    System.Threading.Thread.Sleep(partOfDelay);

                    // Swith the direction of Gnome move
                    // * змінюємо напрям руху гнома
                    --i;
                }
            }
        }

        [TaskNameAttribute("\"Gnome sort\"", LocalName = "Метод \"гнома\"")]
        public static void GnomeSortInTask(int[] arrayToSort,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            for (int i = 0; i < arrayToSort.Length;)
            {
                // Cancel request checking
                // * перевірка на потребу зупинити потік
                if (token.IsCancellationRequested)
                {
                    return;
                }

                // Looking for pairs of neighboring elements situated in wrong order
                // * шукаємо "неправильні" пари сусідів
                if (i == 0 || arrayToSort[i - 1] <= arrayToSort[i])
                {
                    ++i;
                }
                else
                {
                    // The exchange: to correct a "wrong pair" of elements
                    // and the exchange visualization in the main thread
                    // * власне обмін - "виправляємо" знайдену пару елементів
                    // * відображення обміну в головному потоці
                    int t = arrayToSort[i];
                    progress.Report((257, arrayToSort[i]));
                    System.Threading.Thread.Sleep(partOfDelay);

                    arrayToSort[i] = arrayToSort[i - 1];
                    progress.Report((i, arrayToSort[i - 1]));
                    System.Threading.Thread.Sleep(partOfDelay);

                    arrayToSort[i - 1] = t;
                    progress.Report((i - 1, t));
                    System.Threading.Thread.Sleep(partOfDelay);

                    // Swith the direction of Gnome move
                    // * змінюємо напрям руху гнома
                    --i;
                }
            }
        }

        // Improved bubble sort algorithm (compares first element with last one)
        // * впорядкування масиву цілих за зростанням методом зустрічного обміну
        [MethodNameAttribute("Opposite exchange", LocalName = "Зустрічний обмін")]
        public static void OppositeSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            for (int i = 0; i < arrayToSort.Length - 1; ++i)
                for (int j = arrayToSort.Length - 1; j > i; --j)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    // Looking for pairs of elements situated in wrong order
                    // * шукаємо "неправильні" пари елементів
                    if (arrayToSort[i] > arrayToSort[j])
                    {
                        // The exchange: to correct a "wrong pair" of elements
                        // and the exchange visualization in the main thread
                        // * власне обмін - "виправляємо" пару елементів
                        // * відображення обміну в головному потоці
                        int t = arrayToSort[i];
                        worker.ReportProgress(257 * 1000 + arrayToSort[i]);
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[i] = arrayToSort[j];
                        worker.ReportProgress(i * 1000 + arrayToSort[j]);
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j] = t;
                        worker.ReportProgress(j * 1000 + t);
                        System.Threading.Thread.Sleep(partOfDelay);

                    }
                }
        }

        [TaskNameAttribute("Opposite exchange", LocalName = "Зустрічний обмін")]
        public static void OppositeSortInTask(int[] arrayToSort,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            for (int i = 0; i < arrayToSort.Length - 1; ++i)
                for (int j = arrayToSort.Length - 1; j > i; --j)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    // Looking for pairs of elements situated in wrong order
                    // * шукаємо "неправильні" пари елементів
                    if (arrayToSort[i] > arrayToSort[j])
                    {
                        // The exchange: to correct a "wrong pair" of elements
                        // and the exchange visualization in the main thread
                        // * власне обмін - "виправляємо" пару елементів
                        // * відображення обміну в головному потоці
                        int t = arrayToSort[i];
                        progress.Report((257, arrayToSort[i]));
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[i] = arrayToSort[j];
                        progress.Report((i, arrayToSort[j]));
                        System.Threading.Thread.Sleep(partOfDelay);

                        arrayToSort[j] = t;
                        progress.Report((j, t));
                        System.Threading.Thread.Sleep(partOfDelay);

                    }
                }
        }

        // Quick sort algorithm (recursive). The front-end method calls working one,
        // which recursively does all work: separates the array to less and greater
        // part using median element as discriminant, than sorts both parts - less
        // and greater
        // * впорядкування за зростанням масиву цілих методом "швидкого сортування"
        // * front-end метод викликає рекурсивного колегу, який виконує всю роботу:
        // * елемент-дискримінант - медіана масиву; рекурсивне впорядкування лівої
        // * та правої частин масиву
        [MethodNameAttribute("Quick sort", LocalName = "Швидке сортування")]
        public static void QuickSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            QBSort(0, arrayToSort.Length - 1, arrayToSort, worker, e);
        }
        private static bool QBSort(int Low, int High, int[] array,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            bool result = true;
            int Lo = Low;
            int Hi = High;
            int MidValue = array[(Lo + Hi) / 2];
            do
            {   // Looking for a "wrong" value to the left and to the right from the discriminant
                // * шукаємо "неправильні" значення ліворуч та праворуч від дискримінанта
                while (array[Lo] < MidValue) ++Lo;
                while (array[Hi] > MidValue) --Hi;
                if (Lo <= Hi)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return false;
                    }
                    if (Lo < Hi)
                    {
                        // The exchange: moving found values to proper parts of the array
                        // and the exchange visualization in the main thread
                        // * власне обмін - переправляємо знайдені значення в "свою" частину
                        // * відображення обміну в головному потоці
                        int t = array[Lo];
                        worker.ReportProgress(257 * 1000 + array[Lo]);
                        System.Threading.Thread.Sleep(partOfDelay);

                        array[Lo] = array[Hi];
                        worker.ReportProgress(Lo * 1000 + array[Hi]);
                        System.Threading.Thread.Sleep(partOfDelay);

                        array[Hi] = t;
                        worker.ReportProgress(Hi * 1000 + t);
                        System.Threading.Thread.Sleep(partOfDelay);
                    }
                    ++Lo; --Hi;
                }
            }
            while (Lo <= Hi);
            // Sorting the less and the greate part of the array (if it isn't empty)
            // * якщо є ще щось зліва/справа від дискримінанта - впорядковуємо і їх
            if (Hi > Low) result &= QBSort(Low, Hi, array, worker, e);
            if (result && Lo < High) result &= QBSort(Lo, High, array, worker, e);
            return result;
        }

        [TaskNameAttribute("Quick sort", LocalName = "Швидке сортування")]
        public static void QuickSortInTask(int[] arrayToSort,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            QBSort(0, arrayToSort.Length - 1, arrayToSort, token, progress);
        }
        private static bool QBSort(int Low, int High, int[] array,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            bool result = true;
            int Lo = Low;
            int Hi = High;
            int MidValue = array[(Lo + Hi) / 2];
            do
            {   // Looking for a "wrong" value to the left and to the right from the discriminant
                // * шукаємо "неправильні" значення ліворуч та праворуч від дискримінанта
                while (array[Lo] < MidValue) ++Lo;
                while (array[Hi] > MidValue) --Hi;
                if (Lo <= Hi)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (token.IsCancellationRequested)
                    {
                        return false;
                    }
                    if (Lo < Hi)
                    {
                        // The exchange: moving found values to proper parts of the array
                        // and the exchange visualization in the main thread
                        // * власне обмін - переправляємо знайдені значення в "свою" частину
                        // * відображення обміну в головному потоці
                        int t = array[Lo];
                        progress.Report((257, array[Lo]));
                        System.Threading.Thread.Sleep(partOfDelay);

                        array[Lo] = array[Hi];
                        progress.Report((Lo, array[Hi]));
                        System.Threading.Thread.Sleep(partOfDelay);

                        array[Hi] = t;
                        progress.Report((Hi, t));
                        System.Threading.Thread.Sleep(partOfDelay);
                    }
                    ++Lo; --Hi;
                }
            }
            while (Lo <= Hi);
            // Sorting the less and the greate part of the array (if it isn't empty)
            // * якщо є ще щось зліва/справа від дискримінанта - впорядковуємо і їх
            if (Hi > Low) result &= QBSort(Low, Hi, array, token, progress);
            if (result && Lo < High) result &= QBSort(Lo, High, array, token, progress);
            return result;
        }

        // Selection sort algorithm - sorting by finding maximal element
        // * впорядкування масиву цілих за зростанням методом вибору максимального
        [MethodNameAttribute("Selection sort", LocalName = "Вибір максимального")]
        public static void SelectionSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            for (int i = arrayToSort.Length - 1; i > 0; --i)
            {
                // Looking for maximal element index in the unsorted part of the array
                // * шукаємо номер найбільшого значення в масиві
                int indexOfMax = 0;
                for (int j = 1; j <= i; ++j)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (arrayToSort[j] > arrayToSort[indexOfMax])
                        indexOfMax = j;
                }
                if (indexOfMax != i)
                {
                    // The exchange: maximal with last element of the unsorted part
                    // and the exchange visualization in the main thread
                    // * власне обмін - найбільшого з останнім у невпорядкованій частині
                    // * відображення обміну в головному потоці
                    int t = arrayToSort[i];
                    worker.ReportProgress(257 * 1000 + arrayToSort[i]);
                    System.Threading.Thread.Sleep(partOfDelay);

                    arrayToSort[i] = arrayToSort[indexOfMax];
                    worker.ReportProgress(i * 1000 + arrayToSort[indexOfMax]);
                    System.Threading.Thread.Sleep(partOfDelay);

                    arrayToSort[indexOfMax] = t;
                    worker.ReportProgress(indexOfMax * 1000 + t);
                    System.Threading.Thread.Sleep(partOfDelay);
                }
            }
        }

        // Selection sort algorithm - sorting by finding maximal element
        // * впорядкування масиву цілих за зростанням методом вибору максимального
        [TaskNameAttribute("Selection sort", LocalName = "Вибір максимального")]
        public static void SelectionSortInTask(int[] arrayToSort,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            for (int i = arrayToSort.Length - 1; i > 0; --i)
            {
                // Looking for maximal element index in the unsorted part of the array
                // * шукаємо номер найбільшого значення в масиві
                int indexOfMax = 0;
                for (int j = 1; j <= i; ++j)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    if (arrayToSort[j] > arrayToSort[indexOfMax])
                        indexOfMax = j;
                }
                if (indexOfMax != i)
                {
                    // The exchange: maximal with last element of the unsorted part
                    // and the exchange visualization in the main thread
                    // * власне обмін - найбільшого з останнім у невпорядкованій частині
                    // * відображення обміну в головному потоці
                    int t = arrayToSort[i];
                    progress.Report((257, arrayToSort[i]));
                    System.Threading.Thread.Sleep(partOfDelay);

                    arrayToSort[i] = arrayToSort[indexOfMax];
                    progress.Report((i, arrayToSort[indexOfMax]));
                    System.Threading.Thread.Sleep(partOfDelay);

                    arrayToSort[indexOfMax] = t;
                    progress.Report((indexOfMax, t));
                    System.Threading.Thread.Sleep(partOfDelay);
                }
            }
        }

        // Batcher sort algorithm
        // * впорядкування масиву цілих за зростанням методом Бетчера
        [MethodNameAttribute("Batcher\'s sort", LocalName = "Метод Бетчера")]
        public static void BatcherSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            // Calculating a power of 2 - first one greater then the array size
            // * відшукання степені 2 - першої більшої за розмір масиву
            int t = 1;
            while (t < arrayToSort.Length) t <<= 1;
            t >>= 1;
            // Looking over all p-subsequences
            // * перебір усіх р-підпослідовностей
            for (int p = t; p > 0; p >>= 1)
            {
                int q = t;
                int r = 0;
                int d = p;
                // Sorting & mergeing subsequences
                // * впорядкування і злиття підпослідовностей
                do
                {   // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    for (int i = 0; i < arrayToSort.Length - d; ++i)
                    {
                        if (((i & p) == r) && (arrayToSort[i] > arrayToSort[i + d]))
                        {
                            // The exchange of two members of a p-subsequence
                            // and the exchange visualization in the main thread
                            // * власне обмін двох членів р-підпослідовності
                            // * відображення обміну в головному потоці
                            int swap = arrayToSort[i];
                            worker.ReportProgress(257 * 1000 + arrayToSort[i]);
                            System.Threading.Thread.Sleep(partOfDelay);

                            arrayToSort[i] = arrayToSort[i + d];
                            worker.ReportProgress(i * 1000 + arrayToSort[i + d]);
                            System.Threading.Thread.Sleep(partOfDelay);

                            arrayToSort[i + d] = swap;
                            worker.ReportProgress((i + d) * 1000 + swap);
                            System.Threading.Thread.Sleep(partOfDelay);
                        }
                    }
                    d = q - p;
                    q >>= 1;
                    r = p;
                }
                while (d != 0);
            }
        }

        [TaskNameAttribute("Batcher\'s sort", LocalName = "Метод Бетчера")]
        public static void BatcherSortInTask(int[] arrayToSort,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            // Calculating a power of 2 - first one greater then the array size
            // * відшукання степені 2 - першої більшої за розмір масиву
            int t = 1;
            while (t < arrayToSort.Length) t <<= 1;
            t >>= 1;
            // Looking over all p-subsequences
            // * перебір усіх р-підпослідовностей
            for (int p = t; p > 0; p >>= 1)
            {
                int q = t;
                int r = 0;
                int d = p;
                // Sorting & mergeing subsequences
                // * впорядкування і злиття підпослідовностей
                do
                {   // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    for (int i = 0; i < arrayToSort.Length - d; ++i)
                    {
                        if (((i & p) == r) && (arrayToSort[i] > arrayToSort[i + d]))
                        {
                            // The exchange of two members of a p-subsequence
                            // and the exchange visualization in the main thread
                            // * власне обмін двох членів р-підпослідовності
                            // * відображення обміну в головному потоці
                            int swap = arrayToSort[i];
                            progress.Report((257, arrayToSort[i]));
                            System.Threading.Thread.Sleep(partOfDelay);

                            arrayToSort[i] = arrayToSort[i + d];
                            progress.Report((i, arrayToSort[i + d]));
                            System.Threading.Thread.Sleep(partOfDelay);

                            arrayToSort[i + d] = swap;
                            progress.Report((i + d, swap));
                            System.Threading.Thread.Sleep(partOfDelay);
                        }
                    }
                    d = q - p;
                    q >>= 1;
                    r = p;
                }
                while (d != 0);
            }
        }

        // Simple insertion algorithm inserts each element to a proper place of sorted part of the array
        // * впорядкування масиву цілих за зростанням методом простої вставки
        [MethodNameAttribute("Simple insertion", LocalName = "Метод простої вставки")]
        public static void InsertionSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            int valueToInsert = 1;
            for (int i = 1; i < arrayToSort.Length; ++i)
            {
                // Get element to be inserted
                // and visualization of the assignment in the main thread
                // * беремо черговий елемент для всавляння
                // * відображаємо присвоєння в головному потоці
                valueToInsert = arrayToSort[i];
                worker.ReportProgress(257 * 1000 + arrayToSort[i]);
                System.Threading.Thread.Sleep(partOfDelay);

                // Looking for the insertion place
                // * шукаємо місце вставки
                int j = i - 1;
                while (j >= 0 && arrayToSort[j] > valueToInsert)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    // Shift sorted elements to get free place for the valueToInsert
                    // and visualization of this process in the main thread
                    // * присвоєння - звільняє місце для valueToInsert
                    // * відображення присвоєння в головному потоці
                    arrayToSort[j + 1] = arrayToSort[j];
                    worker.ReportProgress((j + 1) * 1000 + arrayToSort[j]);
                    System.Threading.Thread.Sleep(partOfDelay);

                    --j;
                }
                // The insertion and visualisation of it
                // * відображення присвоєння в головному потоці
                // * присвоєння - вставка
                arrayToSort[j + 1] = valueToInsert;
                worker.ReportProgress((j + 1) * 1000 + valueToInsert);
                System.Threading.Thread.Sleep(partOfDelay);
            }
        }

        [TaskNameAttribute("Simple insertion", LocalName = "Метод простої вставки")]
        public static void InsertionSortInBackground(int[] arrayToSort,
           CancellationToken token, IProgress<(int index, int value)> progress)
        {
            int valueToInsert = 1;
            for (int i = 1; i < arrayToSort.Length; ++i)
            {
                // Get element to be inserted
                // and visualization of the assignment in the main thread
                // * беремо черговий елемент для всавляння
                // * відображаємо присвоєння в головному потоці
                valueToInsert = arrayToSort[i];
                progress.Report((257, arrayToSort[i]));
                System.Threading.Thread.Sleep(partOfDelay);

                // Looking for the insertion place
                // * шукаємо місце вставки
                int j = i - 1;
                while (j >= 0 && arrayToSort[j] > valueToInsert)
                {
                    // Cancel request checking
                    // * перевірка на потребу зупинити потік
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    // Shift sorted elements to get free place for the valueToInsert
                    // and visualization of this process in the main thread
                    // * присвоєння - звільняє місце для valueToInsert
                    // * відображення присвоєння в головному потоці
                    arrayToSort[j + 1] = arrayToSort[j];
                    progress.Report((j + 1, arrayToSort[j]));
                    System.Threading.Thread.Sleep(partOfDelay);

                    --j;
                }
                // The insertion and visualisation of it
                // * відображення присвоєння в головному потоці
                // * присвоєння - вставка
                arrayToSort[j + 1] = valueToInsert;
                progress.Report((j + 1, valueToInsert));
                System.Threading.Thread.Sleep(partOfDelay);
            }
        }

        // Heap sort algorithm. This modification don't use any additional memory
        // впорядкування масиву цілих за зростанням пірамідальним сортуванням 
        [MethodNameAttribute("Heap sort (inplace)", LocalName = "Пірамідальне сортування")]
        public static void HeapSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            int n = arrayToSort.Length;
            for (int j = n / 2; j >= 0; --j) rebuild(arrayToSort, j, n - 1, worker);

            for (int j = n - 1; j >= 1; --j)
            {
                // Cancel request checking
                // перевірка на потребу зупинити потік
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                int t = arrayToSort[0];
                worker.ReportProgress(257 * 1000 + arrayToSort[0]);
                System.Threading.Thread.Sleep(partOfDelay);

                arrayToSort[0] = arrayToSort[j];
                worker.ReportProgress(0 * 1000 + arrayToSort[j]);
                System.Threading.Thread.Sleep(partOfDelay);

                arrayToSort[j] = t;
                worker.ReportProgress(j * 1000 + t);
                System.Threading.Thread.Sleep(partOfDelay);

                rebuild(arrayToSort, 0, j - 1, worker);
            }
        }
        // Heap building
        private static void rebuild(int[] A, int f, int d, BackgroundWorker worker)
        {
            int MaxSon = f;
            if (2 * f <= d && A[MaxSon] < A[2 * f]) MaxSon = 2 * f;
            if (2 * f + 1 <= d && A[MaxSon] < A[2 * f + 1]) MaxSon = 2 * f + 1;
            if (MaxSon != f)
            {
                int t = A[f];
                worker.ReportProgress(257 * 1000 + A[f]);
                System.Threading.Thread.Sleep(partOfDelay);

                A[f] = A[MaxSon];
                worker.ReportProgress(f * 1000 + A[MaxSon]);
                System.Threading.Thread.Sleep(partOfDelay);

                A[MaxSon] = t;
                worker.ReportProgress(MaxSon * 1000 + t);
                System.Threading.Thread.Sleep(partOfDelay);

                rebuild(A, MaxSon, d, worker);
            }
        }

        [TaskNameAttribute("Heap sort (inplace)", LocalName = "Пірамідальне сортування")]
        public static void HeapSortInTask(int[] arrayToSort,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            int n = arrayToSort.Length;
            for (int j = n / 2; j >= 0; --j) rebuild(arrayToSort, j, n - 1, progress);

            for (int j = n - 1; j >= 1; --j)
            {
                // Cancel request checking
                // перевірка на потребу зупинити потік
                if (token.IsCancellationRequested)
                {
                    return;
                }
                int t = arrayToSort[0];
                progress.Report((257, arrayToSort[0]));
                System.Threading.Thread.Sleep(partOfDelay);

                arrayToSort[0] = arrayToSort[j];
                progress.Report((0, arrayToSort[j]));
                System.Threading.Thread.Sleep(partOfDelay);

                arrayToSort[j] = t;
                progress.Report((j, t));
                System.Threading.Thread.Sleep(partOfDelay);

                rebuild(arrayToSort, 0, j - 1, progress);
            }
        }
        // Heap building
        private static void rebuild(int[] A, int f, int d, IProgress<(int index, int value)> progress)
        {
            int MaxSon = f;
            if (2 * f <= d && A[MaxSon] < A[2 * f]) MaxSon = 2 * f;
            if (2 * f + 1 <= d && A[MaxSon] < A[2 * f + 1]) MaxSon = 2 * f + 1;
            if (MaxSon != f)
            {
                int t = A[f];
                progress.Report((257, A[f]));
                System.Threading.Thread.Sleep(partOfDelay);

                A[f] = A[MaxSon];
                progress.Report((f, A[MaxSon]));
                System.Threading.Thread.Sleep(partOfDelay);

                A[MaxSon] = t;
                progress.Report((MaxSon, t));
                System.Threading.Thread.Sleep(partOfDelay);
                
                rebuild(A, MaxSon, d, progress);
            }
        }

        // Merge sort algorithm. It is "wrong" in the collection because it uses additional memory
        // * впорядкування масиву цілих за зростанням методом злиття 
        [MethodNameAttribute("Merge sort", LocalName = "Сортування об\'єднанням")]
        public static void MergeSortInBackground(int[] arrayToSort,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            int N = arrayToSort.Length;
            int[] copyA = new int[N];
            int step = 1;
            bool AtoB = true;
            while (step < N)
            {
                // Cancel request checking
                // перевірка на потребу зупинити потік
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (AtoB) BPartialMerge(arrayToSort, copyA, step, worker);
                else BPartialMerge(copyA, arrayToSort, step, worker);
                step *= 2;
                AtoB = !AtoB;
            }
            if (!AtoB) arrayToSort = copyA;
        }
        // Merge sorted parts of the array in a new place
        private static void BPartialMerge(int[] from, int[] to, int step,
            BackgroundWorker worker)
        {
            int n = from.Length;
            int dest = 0;
            int first = 0;
            int second = step;
            int firstStop = step < n ? step : n;
            int secondStop = second + step < n ? second + step : n;
            while (dest < n)
            {
                while (first < firstStop && second < secondStop)
                {
                    // smaller goes first
                    if (from[first] < from[second])
                    {
                        to[dest] = from[first++];
                    }
                    else
                    {
                        to[dest] = from[second++];
                    }
                    worker.ReportProgress(dest * 1000 + to[dest]);
                    System.Threading.Thread.Sleep(partOfDelay);
                    ++dest;
                }
                // copying the rest tail
                while (first < firstStop)
                {
                    to[dest] = from[first++];
                    worker.ReportProgress(dest * 1000 + to[dest]);
                    System.Threading.Thread.Sleep(partOfDelay);
                    ++dest;
                }
                while (second < secondStop)
                {
                    to[dest] = from[second++];
                    worker.ReportProgress(dest * 1000 + to[dest]);
                    System.Threading.Thread.Sleep(partOfDelay);
                    ++dest;
                }
                first = second;
                second += step;
                firstStop = first + step < n ? first + step : n;
                secondStop = second + step < n ? second + step : n;
            }
        }

        [TaskNameAttribute("Merge sort", LocalName = "Сортування об\'єднанням")]
        public static void MergeSortInTask(int[] arrayToSort,
            CancellationToken token, IProgress<(int index, int value)> progress)
        {
            int N = arrayToSort.Length;
            int[] copyA = new int[N];
            int step = 1;
            bool AtoB = true;
            while (step < N)
            {
                // Cancel request checking
                // перевірка на потребу зупинити потік
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (AtoB) BPartialMerge(arrayToSort, copyA, step, progress);
                else BPartialMerge(copyA, arrayToSort, step, progress);
                step *= 2;
                AtoB = !AtoB;
            }
            if (!AtoB) arrayToSort = copyA;
        }
        // Merge sorted parts of the array in a new place
        private static void BPartialMerge(int[] from, int[] to, int step,
            IProgress<(int index, int value)> progress)
        {
            int n = from.Length;
            int dest = 0;
            int first = 0;
            int second = step;
            int firstStop = step < n ? step : n;
            int secondStop = second + step < n ? second + step : n;
            while (dest < n)
            {
                while (first < firstStop && second < secondStop)
                {
                    // smaller goes first
                    if (from[first] < from[second])
                    {
                        to[dest] = from[first++];
                    }
                    else
                    {
                        to[dest] = from[second++];
                    }
                    progress.Report((dest, to[dest]));
                    System.Threading.Thread.Sleep(partOfDelay);
                    ++dest;
                }
                // copying the rest tail
                while (first < firstStop)
                {
                    to[dest] = from[first++];
                    progress.Report((dest, to[dest]));
                    System.Threading.Thread.Sleep(partOfDelay);
                    ++dest;
                }
                while (second < secondStop)
                {
                    to[dest] = from[second++];
                    progress.Report((dest, to[dest]));
                    System.Threading.Thread.Sleep(partOfDelay);
                    ++dest;
                }
                first = second;
                second += step;
                firstStop = first + step < n ? first + step : n;
                secondStop = second + step < n ? second + step : n;
            }
        }

// --------------------------------------------------------------------------------------------------
        // Synchronous versions of the same methods
        // * Ті самі методи. Синхронний варіант
        public static void BubbleSort(int[] arrayToSort)
        {
            for (int i = arrayToSort.Length - 1; i > 0; --i)
                for (int j = 0; j < i; ++j)
                {
                    if (arrayToSort[j] > arrayToSort[j + 1])
                    {
                        int t = arrayToSort[j];
                        arrayToSort[j] = arrayToSort[j + 1];
                        arrayToSort[j + 1] = t;
                    }
                }
        }
        public static void ComboSort(int[] arrayToSort)
        {
            double shrink = 1.3;
            int gap = arrayToSort.Length;
            bool swapped = true;

            while (gap > 1 || swapped)
            {
                gap = (int)(gap / shrink);
                if (gap < 1) gap = 1;
                swapped = false;

                for (int j = 0; j < arrayToSort.Length - gap; ++j)
                {
                    if (arrayToSort[j] > arrayToSort[j + gap])
                    {
                        int t = arrayToSort[j];
                        arrayToSort[j] = arrayToSort[j + gap];
                        arrayToSort[j + gap] = t;

                        swapped = true;
                    }
                }
            }
        }
        public static void GnomeSort(int[] arrayToSort)
        {
            for (int i = 0; i < arrayToSort.Length; )
            {
                if (i == 0 || arrayToSort[i - 1] <= arrayToSort[i])
                    ++i;
                else
                {
                    int t = arrayToSort[i];
                    arrayToSort[i] = arrayToSort[i - 1];
                    arrayToSort[i - 1] = t;
                    --i;
                }
            }
        }
        public static void OppositeSort(int[] arrayToSort)
        {
            for (int i = 0; i < arrayToSort.Length - 1; ++i)
                for (int j = arrayToSort.Length - 1; j > i; --j)
                {
                    if (arrayToSort[i] > arrayToSort[j])
                    {
                        int t = arrayToSort[i];
                        arrayToSort[i] = arrayToSort[j];
                        arrayToSort[j] = t;
                    }
                }
        }
        public static void QuickSort(int[] arrayToSort)
        {
            QSort(0, arrayToSort.Length - 1, arrayToSort);
        }
        private static void QSort(int Low, int High, int[] arrayToSort)
        {
            int Lo = Low;
            int Hi = High;
            int MidValue = arrayToSort[(Lo + Hi) / 2];
            do
            {
                while (arrayToSort[Lo] < MidValue) ++Lo;
                while (arrayToSort[Hi] > MidValue) --Hi;
                if (Lo <= Hi)
                {
                    if (Lo < Hi)
                    {
                        int t = arrayToSort[Lo];
                        arrayToSort[Lo] = arrayToSort[Hi];
                        arrayToSort[Hi] = t;
                    }
                    ++Lo; --Hi;
                }
            }
            while (Lo <= Hi);
            if (Hi > Low) QSort(Low, Hi, arrayToSort);
            if (Lo < High) QSort(Lo, High, arrayToSort);
        }
        public static void SelectionSort(int[] arrayToSort)
        {
            for (int i = arrayToSort.Length - 1; i > 0; --i)
            {
                int indexOfMax = 0;
                for (int j = 1; j <= i; ++j)
                    if (arrayToSort[j] > arrayToSort[indexOfMax])
                        indexOfMax = j;
                if (indexOfMax != i)
                {
                    int t = arrayToSort[i];
                    arrayToSort[i] = arrayToSort[indexOfMax];
                    arrayToSort[indexOfMax] = t;
                }
            }
        }
        public static void BatcherSort(int[] arrayToSort)
        {
            int t = 1;
            while (t < arrayToSort.Length) t <<= 1;
            t >>= 1;
            for (int p = t; p > 0; p >>= 1)
            {
                int q = t; int r = 0; int d = p;
                do
                {
                    for (int i = 0; i < arrayToSort.Length - d; ++i)
                    {
                        if ((i & p) == r)
                            if (arrayToSort[i] > arrayToSort[i + d])
                            {
                                int swap = arrayToSort[i];
                                arrayToSort[i] = arrayToSort[i + d];
                                arrayToSort[i + d] = swap;
                            }
                    }
                    d = q - p; q >>= 1; r = p;
                }
                while (d != 0);
            }
        }
        public static void InsertionSort(int[] arrayToSort)
        {
            int valueToInsert = 1;
            for (int i = 1; i < arrayToSort.Length; ++i)
            {
                valueToInsert = arrayToSort[i];
                int j = i - 1;
                while (j >= 0 && arrayToSort[j] > valueToInsert)
                {
                    arrayToSort[j + 1] = arrayToSort[j];
                    --j;
                }
                arrayToSort[j + 1] = valueToInsert;
            }
        }
        private static void rebuild(int[] A, int f, int d)
        {
            int MaxSon = f;
            if (2 * f <= d && A[MaxSon] < A[2 * f]) MaxSon = 2 * f;
            if (2 * f + 1 <= d && A[MaxSon] < A[2 * f + 1]) MaxSon = 2 * f + 1;
            if (MaxSon != f)
            {
                int swap = A[f];
                A[f] = A[MaxSon];
                A[MaxSon] = swap;
                rebuild(A, MaxSon, d);
            }
        }
        public static void HeapSort(int[] arrayToSort)
        {
            int n = arrayToSort.Length;
            for (int j = n / 2; j >= 0; --j) rebuild(arrayToSort, j, n - 1);

            for (int j = n - 1; j >= 1; --j)
            {
                int swap = arrayToSort[0];
                arrayToSort[0] = arrayToSort[j];
                arrayToSort[j] = swap;
                rebuild(arrayToSort, 0, j - 1);
            }
        }
        private static void PartialMerge(int[] from, int[] to, int step)
        {
            int n = from.Length;
            int dest = 0;
            int first = 0;
            int second = step;
            int firstStop = step < n ? step : n;
            int secondStop = second + step < n ? second + step : n;
            while (dest < n)
            {
                while (first < firstStop && second < secondStop)
                {
                    if (from[first] < from[second]) to[dest++] = from[first++];
                    else to[dest++] = from[second++];
                }
                while (first < firstStop) to[dest++] = from[first++];
                while (second < secondStop) to[dest++] = from[second++];
                first = second;
                second += step;
                firstStop = first + step < n ? first + step : n;
                secondStop = second + step < n ? second + step : n;
            }
        }
        public static void MergeSort(int[] arrayToSort)
        {
            int N = arrayToSort.Length;
            int[] copyA = new int[N];
            int step = 1;
            bool AtoB = true;
            while (step < N)
            {
                if (AtoB) PartialMerge(arrayToSort, copyA, step);
                else PartialMerge(copyA, arrayToSort, step);
                step *= 2;
                AtoB = !AtoB;
            }
            if (!AtoB) arrayToSort = copyA;
        }
    }
}
