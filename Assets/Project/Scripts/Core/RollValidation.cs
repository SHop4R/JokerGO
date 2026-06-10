namespace JokerGO.Core
{
    /// <summary>Outcome of validating a dice roll request; the error is safe to show on screen.</summary>
    public readonly struct RollValidation
    {
        public bool IsValid { get; }
        public string Error { get; }

        private RollValidation(bool isValid, string error)
        {
            IsValid = isValid;
            Error = error;
        }

        public static readonly RollValidation Success = new RollValidation(true, null);

        public static RollValidation Fail(string error) => new RollValidation(false, error);
    }
}
