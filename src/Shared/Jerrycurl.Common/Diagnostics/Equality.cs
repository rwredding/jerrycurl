using System;
using System.Collections.Generic;
using System.Linq;
using Jerrycurl.Collections;

namespace Jerrycurl.Diagnostics
{
    internal struct Equality
    {
        private bool? equals;

        public void Add<T>(T left, T right)
        {
            if (this.equals ?? true)
                this.equals = ((left == null && right == null) || (left?.Equals(right) ?? false));
        }

        public void Add<T>(T left, T right, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            if (this.equals ?? true)
                this.equals = comparer.Equals(left, right);
        }

        public void AddCollection<T>(IReadOnlyCollection<T> left, IReadOnlyCollection<T> right)
        {
            if (this.equals ?? true)
            {
                if (left != null && right != null)
                {
                    foreach (var (l, r) in left.Zip(right))
                    {
                        this.equals = ((l == null && r == null) || (l?.Equals(r) ?? false));

                        if (!this.equals.Value)
                            break;
                    }
                }
                else if (left != null || right != null)
                    this.equals = false;
            }
        }

        public bool ToEquals() => this.equals ?? true;

        public static bool CombineAll<T>(IEnumerable<T> left, IEnumerable<T> right)
        {
            if (left == null ^ right == null)
                return false;
            else if (left == null)
                return true;

            foreach (var (l, r) in left.ZipOuter(right))
            {
                if (l == null ^ r == null)
                    return false;
                else if (l != null && l.Equals(r))
                    return false;
            }

            return true;
        }

        public static bool CombineAll<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
        {
            if (object.ReferenceEquals(left, right))
                return true;
            else if (left == null || right == null)
                return false;
            else if (left.Count != right.Count)
                return false;

            for (int i = 0; i < left.Count; i++)
            {
                if (!object.Equals(left[i], right[i]))
                    return false;
            }

            return true;
        }

        public static bool Combine<TParent, T1, T2>(TParent left, TParent right, Func<TParent, T1> func1, Func<TParent, T2> func2)
        {
            Equality eq = new Equality();

            eq.Add(func1(left), func1(right));
            eq.Add(func2(left), func2(right));

            return eq.ToEquals();
        }

        public static bool Combine<TParent, T1, T2, T3>(TParent left, TParent right, Func<TParent, T1> func1, Func<TParent, T2> func2, Func<TParent, T3> func3)
        {
            Equality eq = new Equality();

            eq.Add(func1(left), func1(right));
            eq.Add(func2(left), func2(right));
            eq.Add(func3(left), func3(right));

            return eq.ToEquals();
        }

        public static bool Combine<TParent, T1, T2, T3, T4>(TParent left, TParent right, Func<TParent, T1> func1, Func<TParent, T2> func2, Func<TParent, T3> func3, Func<TParent, T4> func4)
        {
            Equality eq = new Equality();

            eq.Add(func1(left), func1(right));
            eq.Add(func2(left), func2(right));
            eq.Add(func3(left), func3(right));
            eq.Add(func4(left), func4(right));

            return eq.ToEquals();
        }

        public static bool Combine<TParent, T1, T2, T3, T4, T5>(TParent left, TParent right, Func<TParent, T1> func1, Func<TParent, T2> func2, Func<TParent, T3> func3, Func<TParent, T4> func4, Func<TParent, T5> func5)
        {
            Equality eq = new Equality();

            eq.Add(func1(left), func1(right));
            eq.Add(func2(left), func2(right));
            eq.Add(func3(left), func3(right));
            eq.Add(func4(left), func4(right));
            eq.Add(func5(left), func5(right));

            return eq.ToEquals();
        }

        public static bool Combine<T1>(T1 left1, T1 right1)
        {
            Equality eq = new Equality();

            eq.Add(left1, right1);

            return eq.ToEquals();
        }

        public static bool Combine<T1, T2>(T1 left1, T2 left2, T1 right1, T2 right2)
        {
            Equality eq = new Equality();

            eq.Add(left1, right1);
            eq.Add(left2, right2);

            return eq.ToEquals();
        }

        public static bool Combine<T1, T2, T3>(T1 left1, T2 left2, T3 left3, T1 right1, T2 right2, T3 right3)
        {
            Equality eq = new Equality();

            eq.Add(left1, right1);
            eq.Add(left2, right2);
            eq.Add(left3, right3);

            return eq.ToEquals();
        }

        public static bool Combine<T1, T2, T3, T4>(T1 left1, T2 left2, T3 left3, T4 left4, T1 right1, T2 right2, T3 right3, T4 right4)
        {
            Equality eq = new Equality();

            eq.Add(left1, right1);
            eq.Add(left2, right2);
            eq.Add(left3, right3);
            eq.Add(left4, right4);

            return eq.ToEquals();
        }

        public static bool Combine<T1, T2, T3, T4, T5>(T1 left1, T2 left2, T3 left3, T4 left4, T5 left5, T1 right1, T2 right2, T3 right3, T4 right4, T5 right5)
        {
            Equality eq = new Equality();

            eq.Add(left1, right1);
            eq.Add(left2, right2);
            eq.Add(left3, right3);
            eq.Add(left4, right4);
            eq.Add(left5, right5);

            return eq.ToEquals();
        }

        public static bool Combine<T1, T2, T3, T4, T5, T6>(T1 left1, T2 left2, T3 left3, T4 left4, T5 left5, T6 left6, T1 right1, T2 right2, T3 right3, T4 right4, T5 right5, T6 right6)
        {
            Equality eq = new Equality();

            eq.Add(left1, right1);
            eq.Add(left2, right2);
            eq.Add(left3, right3);
            eq.Add(left4, right4);
            eq.Add(left5, right5);
            eq.Add(left6, right6);

            return eq.ToEquals();
        }

        public static bool Combine<T1, T2, T3, T4, T5, T6, T7>(T1 left1, T2 left2, T3 left3, T4 left4, T5 left5, T6 left6, T7 left7, T1 right1, T2 right2, T3 right3, T4 right4, T5 right5, T6 right6, T7 right7)
        {
            Equality eq = new Equality();

            eq.Add(left1, right1);
            eq.Add(left2, right2);
            eq.Add(left3, right3);
            eq.Add(left4, right4);
            eq.Add(left5, right5);
            eq.Add(left6, right6);
            eq.Add(left7, right7);

            return eq.ToEquals();
        }
    }
}