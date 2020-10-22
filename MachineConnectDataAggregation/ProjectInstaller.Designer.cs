namespace TPMMODetailsUpdation
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise,false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MachineConnectDataAggregationService = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // MachineConnectDataAggregationService
            // 
            this.MachineConnectDataAggregationService.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.MachineConnectDataAggregationService.Password = null;
            this.MachineConnectDataAggregationService.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.DisplayName = "MachineConnectDataAggregationService";
            this.serviceInstaller1.ServiceName = "MachineConnectDataAggregationService";
            this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.MachineConnectDataAggregationService,
            this.serviceInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller MachineConnectDataAggregationService;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
    }
}