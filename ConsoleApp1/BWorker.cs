using System;
using System.Threading;
using System.Threading.Tasks;
using BreweryLib.model;
using BreweryLib.ThreadCollections;

namespace ConsoleApp1
{
    internal class BWorker
    {
        private BoundedBuffer<Bottle> _buf;
        private Semaphore _sem;

        public BWorker()
        {
            _buf = new BoundedBuffer<Bottle>();
            _sem = new Semaphore(1,1);

        }

        public void Start()
        {
            Task.Run(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Bottle b = new Bottle();
                        _sem.WaitOne(); // wait for ready
                        Console.WriteLine(_buf.Peek());
                        _buf.Insert(b);
                        Console.WriteLine("prod buf:" + _buf.Peek());
                        Thread.Sleep(25);
                    }
                }
            );

            Task.Run(() =>
                {
                    for (int i = 0; i < 12; i++)
                    {
                        Console.WriteLine(_buf.Peek());
                        Bottle b = _buf.Take();
                        Console.WriteLine("Cons: " + b + "  peek " + _buf.Peek());
                        _sem.Release();
                    }
                    
                }
            );



        }
    }
}