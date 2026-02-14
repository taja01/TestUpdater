namespace TestCaseManager.Configurations
{
    public class AzureOptions
    {
        [Required(ErrorMessage = "Azure DevOps Organization is required")]
        public string Organization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Azure DevOps Project is required")]
        public string Project { get; set; } = string.Empty;

        [Required(ErrorMessage = "Azure DevOps Personal Access Token is required")]
        public string PersonalAccessToken { get; set; } = string.Empty;
    }
}
