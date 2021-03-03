//
// Copyright 2018-2021 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Veldrid.SceneGraph.Util
{
    /// <summary>
    ///     Implementation of Sorted Multiset based on SO Answer:
    ///     https://stackoverflow.com/questions/2597691/are-there-any-implementations-of-multiset-for-net/36315344
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortedMultiSet<T> : IEnumerable<T>
    {
        private readonly SortedDictionary<T, int> _dict;

        public SortedMultiSet()
        {
            _dict = new SortedDictionary<T, int>();
        }

        public SortedMultiSet(IEnumerable<T> items) : this()
        {
            Add(items);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var kvp in _dict)
                for (var i = 0; i < kvp.Value; i++)
                    yield return kvp.Key;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return _dict.ContainsKey(item);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public void Add(T item)
        {
            if (_dict.ContainsKey(item))
                _dict[item]++;
            else
                _dict[item] = 1;
        }

        public void Add(IEnumerable<T> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public void Remove(T item)
        {
            if (!_dict.ContainsKey(item))
                throw new ArgumentException();
            if (--_dict[item] == 0)
                _dict.Remove(item);
        }

        // Return the last value in the multiset
        public T Peek()
        {
            if (!_dict.Any())
                throw new NullReferenceException();
            return _dict.Last().Key;
        }

        // Return the last value in the multiset and remove it.
        public T Pop()
        {
            var item = Peek();
            Remove(item);
            return item;
        }
    }
}