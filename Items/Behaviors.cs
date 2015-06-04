﻿namespace Items
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A named collection of behaviors.
	/// </summary>
	public class BehaviorsDictionary
		: IDictionary<string, Behavior>
	{
		/// <summary>
		/// The internal behaviors
		/// </summary>
		private List<Behavior> internalBehaviors = new List<Behavior>();

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		public ICollection<string> Keys
		{
			get
			{
				return this.internalBehaviors.Select<Behavior, string>(behavior => behavior.Name).ToList();
			}
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		public ICollection<Behavior> Values
		{
			get { return this.internalBehaviors; }
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public int Count
		{
			get { return this.internalBehaviors.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The behavior</returns>
		public Behavior this[string key]
		{
			get
			{
				return this.internalBehaviors.Single(behavior => behavior.Name == key);
			}

			set
			{
				this.Add(key, value);
			}
		}

		/// <summary>
		/// Adds the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		public void Add(Behavior value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "value must be provided");
			}

			this.Add(value.Name, value);
		}

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <exception cref="System.Exception">
		/// Behavior key is it's name
		/// or
		/// Behavior with that name already exists
		/// </exception>
		public void Add(string key, Behavior value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "value must be provided");
			}

			if (value.Name != key)
			{
				throw new ArgumentException("Behavior key is it's name", "key");
			}

			if (this.internalBehaviors.Any(behavior => behavior.Name == key))
			{
				throw new ArgumentException("Behavior with that name already exists", "key");
			}

			this.internalBehaviors.Add(value);
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
		/// </returns>
		public bool ContainsKey(string key)
		{
			return this.internalBehaviors.Any(behavior => behavior.Name == key);
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </returns>
		public bool Remove(string key)
		{
			return this.internalBehaviors.RemoveAll(behavior => behavior.Name == key) > 0;
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
		/// </returns>
		public bool TryGetValue(string key, out Behavior value)
		{
			if (this.ContainsKey(key))
			{
				value = this[key];
				return true;
			}
			else
			{
				value = null;
				return false;
			}
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public void Add(KeyValuePair<string, Behavior> item)
		{
			this.Add(item.Key, item.Value);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear()
		{
			this.internalBehaviors = new List<Behavior>();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains(KeyValuePair<string, Behavior> item)
		{
			Behavior result;
			this.TryGetValue(item.Key, out result);
			return result == item.Value;
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <exception cref="System.IndexOutOfRangeException">Index out of range</exception>
		public void CopyTo(KeyValuePair<string, Behavior>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array", "array must be provided");
			}

			if (this.internalBehaviors.Count + arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", "Index out of range");
			}

			for (int i = 0; i < array.Length - arrayIndex; i++)
			{
				array[i + arrayIndex] = new KeyValuePair<string, Behavior>(this.internalBehaviors[i].Name, this.internalBehaviors[i]);
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public bool Remove(KeyValuePair<string, Behavior> item)
		{
			// doesnt check whether the item only matches on one, could be bad data
			return this.internalBehaviors.RemoveAll(behavior => behavior.Name == item.Key && behavior == item.Value) > 0;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<string, Behavior>> GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
