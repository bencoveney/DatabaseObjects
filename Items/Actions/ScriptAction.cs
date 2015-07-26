﻿namespace Items
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

	/// <summary>
	/// An action which is a script execution.
	/// </summary>
    [DataContract]
	public class ScriptAction
		: IAction
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ScriptAction"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="scriptName">Name of the script.</param>
		public ScriptAction(string name, string scriptName)
		{
			this.Name = name;
			this.ScriptName = scriptName;
			this.Details = new ImplementationDetailsDictionary();
		}

		/// <summary>
		/// Gets the name of the action.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
        [DataMember]
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the implementation specific details.
		/// </summary>
		/// <value>
		/// The details.
        /// </value>
        [DataMember]
		public ImplementationDetailsDictionary Details
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name the of the action's script file.
		/// </summary>
		/// <value>
		/// The name of the script.
        /// </value>
        [DataMember]
		public string ScriptName
		{
			get;
			private set;
		}
	}
}
