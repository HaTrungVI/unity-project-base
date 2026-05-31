using System;

namespace ProjectBase.Data.Core
{
    public struct DataModuleChangedEvent
    {
        public string ModuleId;
    }

    public struct DataModuleSavedEvent
    {
        public string ModuleId;
    }

    public struct DataModuleSaveFailedEvent
    {
        public string ModuleId;
        public Exception Error;
    }

    public struct DataRegistryLoadedEvent { }

    public struct DataRegistrySavedEvent { }

    public struct DataSyncCompletedEvent
    {
        public string ModuleId;
    }

    public struct DataSyncFailedEvent
    {
        public string ModuleId;
        public string Reason;
    }

    public struct DataRollbackEvent
    {
        public string ModuleId;
    }

    public struct DataConflictEvent
    {
        public string ModuleId;
    }
}
