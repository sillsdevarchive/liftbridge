using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using SIL.LiftBridge.Infrastructure;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.View;

namespace SIL.LiftBridge.Controller
{
	internal sealed class LiftBridgeSynchronizeController : ILiftBridgeController, IDisposable
	{
		private readonly SynchronizeProject _projectSynchronizer;
		private readonly LiftProject _currentLanguageProject;
		private bool _changesReceived;

		public bool ChangesReceived { get { return _changesReceived; } }

		public LiftBridgeSynchronizeController(IDictionary<string, string> options)
		{
			_projectSynchronizer = new SynchronizeProject();

			_currentLanguageProject = new LiftProject(options["-p"]);
			ChorusSystem = new ChorusSystem(_currentLanguageProject.PathToProject, options["-u"]);
			LiftFolder.AddLiftFileInfoToFolderConfiguration(ChorusSystem.ProjectFolderConfiguration);
			ChorusSystem.EnsureAllNotesRepositoriesLoaded();
			MainForm = new Form();
		}

		public void SyncronizeProjects()
		{
			_changesReceived = _projectSynchronizer.SynchronizeLiftProject(MainForm, ChorusSystem, _currentLanguageProject);
		}

		#region ILiftBridgeController implementation

		public Form MainForm { get; private set; }

		public ChorusSystem ChorusSystem { get; private set; }

		public LiftProject CurrentProject
		{
			get { return _currentLanguageProject; }
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~LiftBridgeSynchronizeController()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing,
		/// or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		private bool IsDisposed { get; set; }

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the issue.
		/// </remarks>
		private void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			if (disposing)
			{
				MainForm.Dispose();

				if (ChorusSystem != null)
					ChorusSystem.Dispose();
			}
			MainForm = null;
			ChorusSystem = null;

			IsDisposed = true;
		}

		#endregion
	}
}
