namespace ProjectBase.GameFlow
{
    public struct SceneTransitionStartedEvent
    {
        public string FromScene;
        public string ToScene;
    }

    public struct SceneTransitionCompletedEvent
    {
        public string SceneName;
    }

    public struct BootstrapProgressEvent
    {
        public float Progress;
        public string TaskName;
    }
}
