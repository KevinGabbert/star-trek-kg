using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface IQuadrants : IWrite
    {
        IMap Map { get; set; }
        //IOutputWrite Write { get; set; }
        int Capacity { get; set; }
        int Count { get; }
        Quadrant GetActive();
        bool NoHostiles(List<IShip> hostiles);
        List<IShip> GetHostiles();
        int GetHostileCount();
        void Remove(IEnumerable<IShip> shipsToRemove, Map map);

        /// <summary>
        /// goes through each sector in this quadrant and clears hostiles
        /// </summary>
        /// <returns></returns>
        void RemoveShip(string name);

        void Remove(IShip shipToRemove, Map map);
        bool NotFound(Coordinate coordinate);
        void ClearActive();
        void RemoveFromAnywhere();
        void Add(Quadrant item);
        void AddRange(IEnumerable<Quadrant> collection);
        ReadOnlyCollection<Quadrant> AsReadOnly();
        int BinarySearch(int index, int count, Quadrant item, IComparer<Quadrant> comparer);
        int BinarySearch(Quadrant item);
        int BinarySearch(Quadrant item, IComparer<Quadrant> comparer);
        void Clear();
        bool Contains(Quadrant item);
        //List ConvertAll(Converter<,> converter);
        void CopyTo(Quadrant[] array);
        void CopyTo(int index, Quadrant[] array, int arrayIndex, int count);
        void CopyTo(Quadrant[] array, int arrayIndex);
        bool Exists(Predicate<Quadrant> match);
        Quadrant Find(Predicate<Quadrant> match);
        List<Quadrant> FindAll(Predicate<Quadrant> match);
        int FindIndex(Predicate<Quadrant> match);
        int FindIndex(int startIndex, Predicate<Quadrant> match);
        int FindIndex(int startIndex, int count, Predicate<Quadrant> match);
        Quadrant FindLast(Predicate<Quadrant> match);
        int FindLastIndex(Predicate<Quadrant> match);
        int FindLastIndex(int startIndex, Predicate<Quadrant> match);
        int FindLastIndex(int startIndex, int count, Predicate<Quadrant> match);
        void ForEach(Action<Quadrant> action);
        List<Quadrant>.Enumerator GetEnumerator();
        List<Quadrant> GetRange(int index, int count);
        int IndexOf(Quadrant item);
        int IndexOf(Quadrant item, int index);
        int IndexOf(Quadrant item, int index, int count);
        void Insert(int index, Quadrant item);
        void InsertRange(int index, IEnumerable<Quadrant> collection);
        int LastIndexOf(Quadrant item);
        int LastIndexOf(Quadrant item, int index);
        int LastIndexOf(Quadrant item, int index, int count);
        bool Remove(Quadrant item);
        int RemoveAll(Predicate<Quadrant> match);
        void RemoveAt(int index);
        void RemoveRange(int index, int count);
        void Reverse();
        void Reverse(int index, int count);
        void Sort();
        void Sort(IComparer<Quadrant> comparer);
        void Sort(int index, int count, IComparer<Quadrant> comparer);
        void Sort(Comparison<Quadrant> comparison);
        Quadrant[] ToArray();
        void TrimExcess();
        bool TrueForAll(Predicate<Quadrant> match);
        Quadrant this[int index] { get; set; }
    }
}
