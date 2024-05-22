Here's how I did it, 2024-05 //Fayt64
Downloaded from https://github.com/AngryCarrot789/WPFDarkTheme/releases/tag/v2.0
Thank you AngryCarrot789 !!!


# Unzip the Themes.zip file
The attached Themes.zip file contains (I hope) all of the necessary files for using the theme library. 
Just drag and drop the folder (inside the zip) into your project, reference the aero2 assembly mentioned 
in the repo readme, add the merged resource dictionaries to your app's XAML file, and everything should work

# Reference Aero
For both the new and old versions, you need to reference PresentationFramework.Aero2.dll in order for 
drop shadows to work (and for the app to compile). Right click References in the solution explorer bit, 
Add Reference, goto Assemblies and double click/include PresentationFramework.Aero2

(fayt note: I use the program "Everything" to search for files on windows, searched for PresentationFramework.Aero2.dll and used
the one that looked most promising.)

(fayt note: Dont use copy local as you first getting to know this library... Copy Local should be: No)

# Add XML to App.xml
Add the theme, control colours and controls dictionaries by adding this XML to your App.xaml,
This assumes that you don't change the folder structure used by this project

<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Contains all of the colours and brushes for a theme -->
            <ResourceDictionary Source="Themes/ColourDictionaries/SoftDark.xaml"/>
            <!-- Contains most of the control-specific brushes which reference -->
            <!-- the above theme. I aim for this to contain ALL brushes, not most  -->
            <ResourceDictionary Source="Themes/ControlColours.xaml"/>
            <!-- Contains all of the control styles (Button, ListBox, etc) -->
            <ResourceDictionary Source="Themes/Controls.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>


# Add style to your window
In your window XAML code (where you define height/width, etc), add:

Style="{DynamicResource CustomWindowStyle}"




# New Theme Library
I decided to upgrade the theme library so that, instead of having each theme 
contain all of the control styles, there is instead just a global style file.

I may extract them into their own files at some point, e.g. ButtonStyles.xaml, 
ListBoxStyles.xaml, etc, just so that it's easier to find stuff

## Controls.xaml
Contains all of the styles

## ControlColours.xaml
This is where I (mostly attempted to) keep control-specific brushes and stuff.
However, I still sometimes used the resource keys directly, which is fine because each theme
should contain the exact same resource key names, but their colours should change

I may attempt to make a "LightThemeControlColours" and "DarkThemeControlColours", because sometimes
there are colour differences between light and dark themes that just might not work out and will look weird


