using System;
using System.ComponentModel;
using System.Reflection;

namespace SortWorker
{
    // This class encapsulates all points of setting and using of a BackgroundWorker.
    // An instance of this class interacts with an array of integers has to be sorted,
    // with a background sorting method, and signals by events about execution and
    // completion of the sorting process.  
    /*
     * Клас, що інкапсулює все налаштування та використання aBackgroundWorker.
     * Екземпляр повинен взаємодіяти з масивом цілих чисел, який потрібно відсортувати,
     * з методом сортування (у фоновому режимі) та повідомляти про зміни/завершення
     * процесу сортування відповідними подіями.
     */
    public class BackgroundSorter
    {
        private BackgroundWorker worker;
        private int[] arrayToSort;
        private MethodInfo sortMethod;
        // Events affecting the view:
        // * події, що впливають на відображення:
        // - exchange of two array elements
        // *  обмін двох значень
        public event SortingExchangeEventHandler SortingExchange;
        // - completion of the sorting process
        // *  завершення сортування
        public event SortingCompleteEventHandler SortingComplete;

        // delegates for own events
        // * делегати для власних подій
        public delegate void SortingExchangeEventHandler(Object sender,
            SortingExchangeEventArgs e);
        public delegate void SortingCompleteEventHandler(Object sender,
            SortingCompleteEventArgs e);

        public BackgroundSorter(int[] array, MethodInfo theMethod)
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            // Set-up required methods to interact with the backgroundWorker:
            // * для налаштування взаємодії потрібно задати три методи:
            // - main long term work
            // * основну довготривалу роботу
            worker.DoWork += worker_DoWork;
            // - action after main work completion
            // * завершальні дії після завершення основної роботи
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            // - displaying of main work progress
            // * метод інформування про хід виконання основної роботи
            worker.ProgressChanged += worker_ProgressChanged;

            this.arrayToSort = array;
            this.sortMethod = theMethod;
        }

        public BackgroundSorter(int[] array)
            : this(array, null)
        {
        }

        // event dispatchers
        // диспетчери подій
        private void OnSortingExchange(int i, int j)
        {
            if (SortingExchange != null)
                SortingExchange(this, new SortingExchangeEventArgs(i, j));
        }
        private void OnSortingComplete(bool canceled)
        {
            if (SortingComplete != null)
                SortingComplete(this, new SortingCompleteEventArgs(canceled));
        }

        // задати масив поза конструктором потрібно тоді, коли ви вирішили відсортувати
        // новий масив тим самим методом у тому ж потоці
        public int[] ArrayToSort
        {
            get
            {
                return arrayToSort;
            }
            set
            {
                arrayToSort = value;
            }
        }
        public void SetMethod(MethodInfo theMethod)
        {
            this.sortMethod = theMethod;
        }

        // access to the backgroundWorker interface
        // доступ до інтерфейсу асинхронного шаблона
        public void Execute()
        {
            worker.RunWorkerAsync(arrayToSort);
        }
        public void Stop()
        {
            worker.CancelAsync();
        }

        // The backgroundWorker events handlers:
        // * методи опрацювання подій асинхронного шаблона:
        // - long term execution - the array sorting. We launch it by aMethodInfo invokation
        // *  "основний цикл" - сортування. Запускаємо його за допомогою aMethodInfo.Invoke
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (sortMethod != null && arrayToSort != null)
            {
                sortMethod.Invoke(null, new object[] { (int[])e.Argument, sender as BackgroundWorker, e });
            }
            else throw new NullReferenceException("Trying to use null array or null sorting method");
        }
        // - handler of completion of the sorting process
        // *  на завершення перевіряєм коректність завершення,
        // *  сигналізуємо про завершення
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null) System.Windows.Forms.MessageBox.Show(e.Error.Message);
            OnSortingComplete(e.Cancelled);
        }
        // - handler of sorting progress reports indexes of exchanged elements
        // *  візуалізація обміну двох цілих значень
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // номери елементів запаковані в property ProgressPercentage
            int i = e.ProgressPercentage / 1000;
            int j = e.ProgressPercentage % 1000;
            OnSortingExchange(i, j);
        }
    }

    // own event argument types
    // * типи для аргументів власних подій
    public class SortingExchangeEventArgs : EventArgs
    {
        public int FirstIndex { get; set; }
        public int SecondIndex { get; set; }
        public SortingExchangeEventArgs(int i, int j)
        {
            FirstIndex = i;
            SecondIndex = j;
        }
    }
    public class SortingCompleteEventArgs : EventArgs
    {
        public bool Canceled { get; set; }
        public SortingCompleteEventArgs(bool state)
        {
            Canceled = state;
        }
    }
}
