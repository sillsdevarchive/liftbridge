using System;
using System.Windows.Forms;
using Chorus.UI.Clone;

namespace SIL.LiftBridge.View
{
	/// <summary>
	/// This control is used by the LiftBridgeDlg when there is no extant Hg repo for some FW language project.
	///
	/// The Startup event lets the parent dlg know what the user wants to do (use extant repo or make new one).
	/// </summary>
	internal sealed partial class StartupNew : UserControl, IStartupNewView
	{
		#region IStartupNewView

		public event StartupNewEventHandler Startup;

		#endregion IStartupNewView

		internal StartupNew()
		{
			InitializeComponent();
		}

		private void RadioButtonClicked(object sender, EventArgs e)
		{
			UpdateDisplayStatus();
		}

		private void CheckBoxChanged(object sender, EventArgs e)
		{
			UpdateDisplayStatus();
		}

		private void UpdateDisplayStatus()
		{
			groupBox1.Enabled = _rbUseExistingSystem.Checked;
			if (!groupBox1.Enabled)
			{
				_rbUsb.Checked = false;
				_rbLocalNetwork.Checked = false;
				_rbInternet.Checked = false;

				_cbImportWarning.Checked = false;
			}
			_cbImportWarning.Enabled = _rbUseExistingSystem.Checked && IsOneGetFromRadioButtonChecked;

			_btnContinue.Enabled = _rbFirstToUseFlexBridge.Checked
				|| (_rbUseExistingSystem.Checked
						&& IsOneGetFromRadioButtonChecked
						&& _cbImportWarning.Checked);
		}

		private bool IsOneGetFromRadioButtonChecked
		{
			get { return _rbUsb.Checked || _rbLocalNetwork.Checked || _rbInternet.Checked; }
		}

		private void ContinueBtnClicked(object sender, EventArgs e)
		{
			var sharedSystemType = (_rbFirstToUseFlexBridge.Checked) ? SharedSystemType.New : SharedSystemType.Extant;

			var repoSource = _rbUsb.Checked
				? ExtantRepoSource.Usb
				: (_rbLocalNetwork.Checked
					? ExtantRepoSource.LocalNetwork
					: ExtantRepoSource.Internet);

			OnStartup(new StartupNewEventArgs(sharedSystemType, repoSource));
		}

		private void OnStartup(StartupNewEventArgs e)
		{
			if (Startup != null)
				Startup(this, e);
		}

		private void CloseButtonClick(object sender, EventArgs e)
		{
			CloseApp(this, e);
		}

		#region Implementation of IActiveView

		public event EventHandler CloseApp;

		#endregion
	}
}
