using System;
using System.Linq;
using System.Collections.Generic;

namespace Gmail2.Server
{
    public class RandomGenerator
    {
        private Random _random;

        private DateTime _now;

        public RandomGenerator()
        {
            _random = new Random();
            _now = DateTime.Now;
        }

        public double GetRandomDouble()
        {
            return _random.NextDouble();
        }

        public T GetRandom<T>(T[] items)
        {
            int number = _random.Next(1, 100000) % items.Length;
            return items[number];
        }

        public KeyValuePair<T1, T2> GetRandom<T1, T2>(Dictionary<T1, T2> dict)
        {
            var keys = dict.Keys.Cast<T1>().ToArray();
            var item = GetRandom(keys);

            return new KeyValuePair<T1, T2>(item, dict[item]);
        }

        public T GetRandomOrNull<T>(T[] items)
        {
            int number = _random.Next(1, 100000) % (items.Length + 1);

            if (number < items.Length)
                return items[number];
            else
                return default(T);
        }

        public T GetRandom<T>(List<T> items)
        {
            int number = _random.Next(1, 100000) % items.Count;
            return items[number];
        }

        public T GetRandomEnumValue<T>()
        {
            var names = Enum.GetNames(typeof(T));
            string name = GetRandom(names);
            T result = (T)Enum.Parse(typeof(T), name);
            return result;
        }

        public int GetRandomNumber(int start, int end)
        {
            return _random.Next(start, end);
        }

        public bool GetRandomBool()
        {
            int number = _random.Next(1, 100000);
            return number % 2 == 0;
        }

        public DateTime GetRandomDate()
        {
            int days = 0;

            if (GetRandomBool())
                days = _random.Next(1000000, 9999999) % 700;
            else
                days = _random.Next(1000000, 9999999) % 28;

            return _now.AddDays(days * -1);
        }

        public DateTime? GetRandomDateOrNull()
        {
            int days = _random.Next(1, 700);
            if (days % 5 == 0)
                return null;
            else
                return _now.AddDays(days * -1);
        }

        public DateTime? GetRandomDateNullable()
        {
            if (GetRandomBool())
            {
                int days = _random.Next(1, 700);
                return _now.AddDays(days * -1);
            }
            else
            {
                return null;
            }
        }

        public int? GetRandomNumberOrNull(int min, int max)
        {
            if (GetRandomBool())
                return _random.Next(min, max);
            else
                return null;
        }

        public DateTime? GetRandomDateOrNull(DateTime min, DateTime max)
        {
            if (GetRandomBool())
                return null;

            var span = max.Subtract(min);
            return min.AddDays(span.TotalDays);
        }

        public List<T> GetRandomMany<T>(IEnumerable<T> items, int count)
        {
            if (count >= items.Count())
                return items.ToList();

            return items.OrderBy(x => _random.Next()).Take(count).ToList();
        }
    }
}
