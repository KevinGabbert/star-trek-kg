using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StarTrek_KG.Playfield;

namespace StarTrek_KG.Interfaces
{
    public interface IRegions : IWrite
    {
        IMap Map { get; set; }
        //IOutputWrite Write { get; set; }
        int Capacity { get; set; }
        int Count { get; }
        Region GetActive();
        bool NoHostiles(List<IShip> hostiles);
        List<IShip> GetHostiles();
        int GetHostileCount();
        void Remove(IEnumerable<IShip> shipsToRemove, Map map);

        /// <summary>
        /// goes through each sector in this Region and clears hostiles
        /// </summary>
        /// <returns></returns>
        void RemoveShip(string name);

        void Remove(IShip shipToRemove, Map map);
        bool NotFound(Coordinate coordinate);
        void ClearActive();
        void RemoveFromAnywhere();
        void Add(Region item);
        void AddRange(IEnumerable<Region> collection);
        ReadOnlyCollection<Region> AsReadOnly();
        int BinarySearch(int index, int count, Region item, IComparer<Region> comparer);
        int BinarySearch(Region item);
        int BinarySearch(Region item, IComparer<Region> comparer);
        void Clear();
        bool Contains(Region item);
        //List ConvertAll(Converter<,> converter);
        void CopyTo(Region[] array);
        void CopyTo(int index, Region[] array, int arrayIndex, int count);
        void CopyTo(Region[] array, int arrayIndex);
        bool Exists(Predicate<Region> match);
        Region Find(Predicate<Region> match);
        List<Region> FindAll(Predicate<Region> match);
        int FindIndex(Predicate<Region> match);
        int FindIndex(int startIndex, Predicate<Region> match);
        int FindIndex(int startIndex, int count, Predicate<Region> match);
        Region FindLast(Predicate<Region> match);
        int FindLastIndex(Predicate<Region> match);
        int FindLastIndex(int startIndex, Predicate<Region> match);
        int FindLastIndex(int startIndex, int count, Predicate<Region> match);
        void ForEach(Action<Region> action);
        List<Region>.Enumerator GetEnumerator();
        List<Region> GetRange(int index, int count);
        int IndexOf(Region item);
        int IndexOf(Region item, int index);
        int IndexOf(Region item, int index, int count);
        void Insert(int index, Region item);
        void InsertRange(int index, IEnumerable<Region> collection);
        int LastIndexOf(Region item);
        int LastIndexOf(Region item, int index);
        int LastIndexOf(Region item, int index, int count);
        bool Remove(Region item);
        int RemoveAll(Predicate<Region> match);
        void RemoveAt(int index);
        void RemoveRange(int index, int count);
        void Reverse();
        void Reverse(int index, int count);
        void Sort();
        void Sort(IComparer<Region> comparer);
        void Sort(int index, int count, IComparer<Region> comparer);
        void Sort(Comparison<Region> comparison);
        Region[] ToArray();
        void TrimExcess();
        bool TrueForAll(Predicate<Region> match);
        Region this[int index] { get; set; }
    }
}
