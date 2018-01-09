using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// HashSet<T>-esque implementation that preserves insertion order, backed
/// by a HashSet<T> and LinkedList<T>. Similar to Java's LinkedHashSet.
///
/// The methods below should map 1-1 with the HashSet<T> public interface,
/// and ISet<T> is implemented, but Unity's old school mono does not yet
/// include ISet.
///
/// <author>dadler@ea.com</author>
/// </summary>


public class LinkedHashSet<T> : /* ISet<T>, */ ICollection<T>, IEnumerable<T>, IEnumerable {

    // For linked list operations    
    private readonly LinkedList<T> _list = new LinkedList<T>();
    // For set operations
    private readonly HashSet<T> _set;
    // For optimizations on Remove()
    private readonly Dictionary<T, LinkedListNode<T>> _nodeMap;

    public int Count {
        get { return _set.Count; }
    }

    public IEqualityComparer<T> Comparer { 
        get { return _set.Comparer; }
    }

    public LinkedHashSet() {
        _set = new HashSet<T>();
        _nodeMap = new Dictionary<T, LinkedListNode<T>>();
    }

    public LinkedHashSet( IEnumerable<T> collection ) : base() {
        UnionWith( collection );
    }

    public LinkedHashSet( IEqualityComparer<T> comparer ) {
        _set = new HashSet<T>( comparer );
        _nodeMap = new Dictionary<T, LinkedListNode<T>>( comparer );
    }

    public LinkedHashSet( IEnumerable<T> collection, IEqualityComparer<T> comparer ) {
        _set = new HashSet<T>( comparer );
        _nodeMap = new Dictionary<T, LinkedListNode<T>>( comparer );
        UnionWith( collection );
    }

    public bool Contains( T item ) {
        return _set.Contains( item );
    }

    public bool Add( T item ) {
		if ( _set.Add( item ) ) { // item was not a member of the HashSet and has been added
            // Add to both list and nodeSet
            _nodeMap.Add( item, _list.AddLast( item ) );
			return true;
		}
		return false;
    }

	public void Clear() {
		_set.Clear();
		_list.Clear();
        _nodeMap.Clear();
	}

    public bool Remove( T item ) {
        if ( _set.Remove( item ) ) { // item was a member of the HashSet and has been removed
            ListRemove( item );
            return true;
        }
        return false;        
    }

    private void ListRemove( T item ) {
        LinkedListNode<T> node;
        // Try to use nodeMap to optimize Remove instead of full linear search
        if ( _nodeMap.TryGetValue( item, out node ) ) {
            _list.Remove( node );
            _nodeMap.Remove( item );
        } else {
            _list.Remove( item );
        }
    }

    public bool SetEquals( IEnumerable<T> other ) {
        return _set.SetEquals( other );
    }

    public void UnionWith( IEnumerable<T> other ) {
        foreach( T item in other ) Add( item );
    }

    public void IntersectWith( IEnumerable<T> other ) {
        // Remove other items not in hashSet from list
        foreach( T item in other ) 
            if ( !_set.Contains( item ) ) ListRemove( item );

        HashSet<T> otherSet = new HashSet<T>( other );
        // Remove hashSet items not in otherSet from list
        foreach( T item in _set ) 
            if ( !otherSet.Contains( item ) ) ListRemove( item );

        // Apply HashSet impl
        _set.IntersectWith( other );        
    }

    // Symmetric difference / XOR membership
    public void SymmetricExceptWith( IEnumerable<T> other ) {
        // Remove other items in hashSet from list
        foreach( T item in other ) 
            if ( _set.Contains( item ) ) ListRemove( item );

        HashSet<T> otherSet = new HashSet<T>( other );
        // Remove hashSet items in otherSet from list
        foreach( T item in _set ) 
            if ( otherSet.Contains( item ) ) ListRemove( item );

        // Apply HashSet impl
        _set.SymmetricExceptWith( other ); 
    }

    public bool IsSubsetOf( IEnumerable<T> other ) {
        return _set.IsSubsetOf( other );
    }

    public bool IsSupersetOf( IEnumerable<T> other ) {
        return _set.IsSupersetOf( other );
    }

    public bool IsProperSubsetOf( IEnumerable<T> other ) {
        return _set.IsProperSubsetOf( other );
    }

    public bool IsProperSupersetOf( IEnumerable<T> other ) {
        return _set.IsProperSupersetOf( other );
    }

    public bool Overlaps( IEnumerable<T> other ) {
        return _set.Overlaps( other );
    }

    public int RemoveWhere( Predicate<T> match ) {

        // Apply predicate to list        
        foreach( T item in _set )
            if ( match(item) )
                ListRemove( item );

        // Use HashSet impl directly
        return _set.RemoveWhere( match );
    }

    public void TrimExcess() {
        _set.TrimExcess();
    }

    bool ICollection<T>.IsReadOnly { 
        get { return ((ICollection<T>)_set).IsReadOnly; }
    }

    void ICollection<T>.Add( T item ) {
        Add( item );
    }

    public IEnumerator<T> GetEnumerator() {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public IEnumerable<T> GetReverseEnumerator() {
        LinkedListNode<T> node = _list.Last;
        while ( node != null ) {
            yield return node.Value;
            node = node.Previous;
        }
    }

    public List<T> GetList() {
        return new List<T>( _list );
    }

    public List<T> GetListReversed() {
        List<T> result = GetList();
        result.Reverse();
        return result;
    }
    
    public LinkedList<T> GetLinkedList() {
        return _list;   
    }
    
    public HashSet<T> GetHashSet() {
        return _set;   
    }
    
    // Use inner set for equality
    public override bool Equals( Object obj ) {
        return _set.Equals( obj );
    }

    public override int GetHashCode() {
        return _set.GetHashCode();
    }

    /// <summary>
    /// Copies LinkedHashSet to the given array.
    /// NOTE: This method does not preserve insertion order. It is a pass-through
    /// to HashSet.CopyTo().
    /// </summary>
    /// <param name="array">Array.</param>
    public void CopyTo( T[] array ) {
        _set.CopyTo( array );
    }

    /// <summary>
    /// Copies LinkedHashSet to the given array.
    /// NOTE: This method does not preserve insertion order. It is a pass-through
    /// to HashSet.CopyTo().
    /// </summary>
    /// <param name="array">Array.</param>
    public void CopyTo( T[] array, int arrayIndex ) {
        _set.CopyTo( array, arrayIndex );
    }

    /// <summary>
    /// Copies LinkedHashSet to the given array.
    /// NOTE: This method does not preserve insertion order. It is a pass-through
    /// to HashSet.CopyTo().
    /// </summary>
    /// <param name="array">Array.</param>
    public void CopyTo( T[] array, int arrayIndex, int count ) {
        _set.CopyTo( array, arrayIndex, count );
    }
}