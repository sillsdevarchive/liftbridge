using System;
using System.Windows.Forms;

namespace LiftBridgeCore
{
	public delegate void ExportLexiconEventHandler(object sender, LiftBridgeEventArgs e);
	public delegate void ImportLexiconEventHandler(object sender, LiftBridgeEventArgs e);
	public delegate void BasicLexiconImportEventHandler(object sender, LiftBridgeEventArgs e);

	/// <summary>
	/// This interface defines events that a LiftBridge client can implement handlers for.
	/// This allows for clients that do not deal directly with LIFT files as a primary data source
	/// to collaborate with other software that uses LIFT data.
	/// </summary>
	public interface ILiftBridge : IDisposable
	{
		/// <summary>
		/// Export the internally held lexicon into the LIFT file given in LiftBridgeEventArgs.
		/// Handlers should create the file, if needed.
		/// </summary>
		event ExportLexiconEventHandler ExportLexicon;

		/// <summary>
		/// Import the LIFT file into the internally held lexicon.
		/// Entries in an internal lexicon that are not in the Lift file are removed.
		/// </summary>
		event ImportLexiconEventHandler ImportLexicon;

		/// <summary>
		/// Do a basic 'safe' import, where entries in the internally held lexicon
		/// that are not in the Lift file are not removed.
		/// </summary>
		event BasicLexiconImportEventHandler BasicLexiconImport;

		/// <summary>
		/// Do the Send/Receive for the given language project name.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <param name="projectName"/> is null or an empty string.
		/// </exception>
		void DoSendReceiveForLanguageProject(Form parent, string projectName);
	}

	/// <summary>
	/// Extend ILiftBridge to deal with repo identifier.
	///
	/// Client will store the id when Lift Bridge creates the repos, or gets in in the first sync.
	/// Client will then set this property before calling the DoSendReceiveForLanguageProject method of the base ILiftBridge interface.
	///
	/// Lift Bridge will use the identifier to locate the matching repo in the Lift Bridge storage location.
	/// </summary>
	public interface ILiftBridge2 : ILiftBridge
	{
		/// <summary>
		/// Gets or sets the identifier for the repository.
		/// </summary>
		string RepositoryIdentifier { get; set; }
	}
}
