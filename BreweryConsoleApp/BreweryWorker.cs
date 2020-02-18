using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BreweryLib.model;
using BreweryLib.ThreadCollections;

namespace BreweryConsoleApp
{
    class BreweryWorker
    {
        // Constants
        private const int NUMBER_OF_BOTTLES = 3000;
        private const int BUFFER_LENGTH = NUMBER_OF_BOTTLES / 2;
        private const int Sampling_Number = 149;
        private const int NUMBER_OF_WASHING_THREADS = 3;
        private const int NUMBER_OF_FILLING_TOPPING_THREADS = 8;
        private const int NUMBER_OF_PACKING_THREADS = 2;

        // Critical regions between threads
        private readonly BoundedBuffer<Bottle> unwashedBottles;
        private readonly BoundedBuffer<Bottle> washedBottles;
        private readonly BoundedBuffer<Bottle>[] filledBottles;
        private readonly Semaphore[] toppedIsReady;
        private readonly BoundedBuffer<Bottle> toppedBottles;
        private readonly BoundedBuffer<List<Bottle>> boxOfBottles;




        // helping tools
        private readonly Random rnd;

        public BreweryWorker()
        {
            rnd = new Random(DateTime.Now.Millisecond);

            unwashedBottles = new BoundedBuffer<Bottle>(BUFFER_LENGTH);
            washedBottles = new BoundedBuffer<Bottle>(BUFFER_LENGTH);
            filledBottles = new BoundedBuffer<Bottle>[NUMBER_OF_FILLING_TOPPING_THREADS];
            toppedIsReady = new Semaphore[NUMBER_OF_FILLING_TOPPING_THREADS];
            for (int i = 0; i < NUMBER_OF_FILLING_TOPPING_THREADS; i++)
            {
                filledBottles[i] = new BoundedBuffer<Bottle>(1);
                toppedIsReady[i] = new Semaphore(1,1); 
            }
            toppedBottles = new BoundedBuffer<Bottle>(BUFFER_LENGTH);
            boxOfBottles = new BoundedBuffer<List<Bottle>>(BUFFER_LENGTH/24);
        }

        public void Start()
        {
            Trace.TraceInformation("Simulation Started");

            // Start generate bottles
            Thread tg = new Thread(() => GenerateBottles(NUMBER_OF_BOTTLES));
            tg.Start();

            // Start washing bottles
            Task.Run(() =>
            {
                Trace.TraceInformation($"{NUMBER_OF_WASHING_THREADS} washing threads started");
                Parallel.For(0, NUMBER_OF_WASHING_THREADS, (i) => WashBottle(i));
            });

            // Start Filling bottles
            Task.Run(() =>
            {
                Trace.TraceInformation($"{NUMBER_OF_FILLING_TOPPING_THREADS} filling threads started");
                Parallel.For(0, NUMBER_OF_FILLING_TOPPING_THREADS, (i) => FillBottle(i));
            });

            // Start Topping bottles
            Task.Run(() =>
            {
                Trace.TraceInformation($"{NUMBER_OF_FILLING_TOPPING_THREADS} topping threads started");
                Parallel.For(0, NUMBER_OF_FILLING_TOPPING_THREADS, (i) => TopBottle(i));
            });

            // Start Packing bottles
            Task.Run(() =>
            {
                Trace.TraceInformation($"{NUMBER_OF_PACKING_THREADS} packing threads started");
                Parallel.For(0, NUMBER_OF_PACKING_THREADS, (i) => BoxingBottle(i));
            });

            /*
             * Get Boxes
             */
            int expectedNoOfBoxes = NUMBER_OF_BOTTLES / 24 - NUMBER_OF_PACKING_THREADS;
            int countedBoxes = 0;
            while (countedBoxes <= expectedNoOfBoxes)
            {
                List<Bottle> box = boxOfBottles.Take();
                Trace.TraceInformation($"Box no {++countedBoxes} received with {box.Count} bottles");
            }
            Trace.TraceInformation("Simulation ended");

            // nice close of all tracing
            foreach (TraceListener tl in Trace.Listeners)
            {
                tl?.Close();
            }

            Thread.Sleep(10000);
            System.Environment.Exit(0);
        }

        /*
         * Packing Bottles
         */
        private void BoxingBottle(int nr)
        {
            String myName = "Counting" + nr;
            Trace.TraceInformation($"{myName} is started");

            List<Bottle> bottleBox;
            while (true)
            {
                bottleBox = new List<Bottle>();
                for (int i = 0; i < 24; i++)
                {
                    Bottle b = toppedBottles.Take();
                    Thread.Sleep(5 + rnd.Next(10));
                    b.State = "Boxing";
                    bottleBox.Add(b);
                    TraceBottle(b);
                }
                boxOfBottles.Insert(bottleBox);
            }
        }

        /*
         * Topping Bottles
         */
        private void TopBottle(int nr)
        {
            String myName = "Top" + nr;
            Trace.TraceInformation($"{myName} is started");

            while (true)
            {
                Bottle b = filledBottles[nr].Take();
                Thread.Sleep(15 + rnd.Next(10));
                b.State = "Topped";
                TraceBottle(b);
                toppedBottles.Insert(b);
                toppedIsReady[nr].Release();
            }

        }

        /*
         * Filling Bottles
         */
        private void FillBottle(int nr)
        {
            String myName = "Fill" + nr;
            Trace.TraceInformation($"{myName} is started");

            while (true)
            {
                toppedIsReady[nr].WaitOne();
                Bottle b = washedBottles.Take();
                Thread.Sleep(15 + rnd.Next(15));
                b.State = "Filled";
                TraceBottle(b);
                filledBottles[nr].Insert(b);
            }

        }


        /*
        * Generate Bottles
        */
        private void GenerateBottles()
        {GenerateBottles(Int32.MaxValue);}
        private void GenerateBottles(int number)
        {
            Bottle bottle;
            for (int i = 0; i < number; i++)
            {
                bottle = new Bottle();
                unwashedBottles.Insert(bottle);
                TraceBottle(bottle);
                Thread.Sleep(10 + rnd.Next(10));
            }
        }

        /*
         * Washing Bottles
         */
        private void WashBottle(int nr)
        {
            String myName = "Wash" + nr;
            Trace.TraceInformation($"{myName} is started");

            while (true)
            {
                Bottle b = unwashedBottles.Take();
                Thread.Sleep(5+rnd.Next(15));
                b.State = "washed";
                TraceBottle(b);
                washedBottles.Insert(b);
            }
        }



        private void TraceBottle(Bottle b)
        {
            if (b.Id % Sampling_Number == 0)
            {
                Trace.TraceInformation("Sampling " + b);
            }
        }
    }
}