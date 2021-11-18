using System.Collections.Generic;
using System.ComponentModel;

namespace FastGitBranchX64
{
    internal partial class OptionsProvider
    {
        // Register the options with these attributes on your package class:
        //[ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "FastGitBranch", "Test", 0, 0, true)]
        //[ProvideProfile(typeof(OptionsProvider.GeneralOptions), "FastGitBranch", "Test", 0, 0, true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        //[Category("General")]
        //[DisplayName("Number of branch name parts")]
        //[Description("Branch name will be composed of n+1 parts for example selectedPart1\\selectedPart2\\whatYouType.")]
        //[DefaultValue(2)]
        //public int NumberOfBranchNameParts { get; set; } = 2;

        [Category("General")]
        [DisplayName("Branch Name First Part")]
        [Description("List of options in first part of branch name.")]
        
        [TypeConverter(typeof(ArrayConverter))]
        public string[] FirstPart { get; set; }

        [Category("General")]
        [DisplayName("Branch Name Second Part")]
        [Description("List of options in second part of branch name.")]

        [TypeConverter(typeof(ArrayConverter))]
        public string[] SecondPart { get; set; }

        public General()
        {
            FirstPart = new string[] { "feature", "hotfix", "release", "upgrade" };
            SecondPart = new string[] { "" };
        }



    }

}
