# Profile Actions

Profile action is a special type of [actions with parameters](../Add-a-command-with-a-parameter/index.html) , but there are some key differences.

With profile actions:

- UI does not show all possible parameter values in the actions list;
- in most cases it is not possible to predict the parameter list;
- parameters are entered by users in UI;
- profile actions with actual parameters are stored in application profiles.

They are called *profile* actions because the actual profile actions are stored in application *profiles* .

Existing [actions with parameters](../Add-a-command-with-a-parameter/index.html) can be easily converted to profile actions by calling the `MakeProfileAction()` method in the dynamic action constructor.

## Profile action types

- `"text"` - any text, is represented in UI as a label and a text box.
- `"execute"` - path to the executable file to run.
- `"list"` - an item from the list, is represented in the UI as a label and a combo box.
- `"tree"` - multi-level selection; currently only 2-level combo boxes are supported.

The plugin can provide data for "list" and "tree" actions by overriding the `GetProfileActionData()` method of dynamic action.

### Labels

The profile action type can be complemented with a label to show in UI, e.g.:

- "text;Enter chat message to send:"
- "list;Select album to play:"

## Usage

### Create profile action ("text")

As an example, we will implement a profile action that sends a user-defined chat message.

1. In the plugin project, create a class based on either `PluginDynamicCommand` or `PluginDynamicAdjustment` base class.
2. Call the `MakeProfileAction` method in the class constructor: `this . MakeProfileAction ( "text;Enter chat message to send:" );`
3. Add code to execute the command. In this example, it is as simple as starting the application URI: `protected override void RunCommand ( String actionParameter ) => Chat . SendMessage ( actionParameter );`

### Create profile action ("list")

As an example, we will implement a profile action that shows selected parameter.

1. In the plugin project, create a class based on either `PluginDynamicCommand` or `PluginDynamicAdjustment` base class: `public class DynamicListProfileAction : PluginDynamicCommand { public DynamicListProfileAction () { this . DisplayName = "Dynamic List" ; this . GroupName = "Profile Actions" ; for ( var i = 0 ; i < 5 ; i ++ ) { this . AddParameter ( $"item{i}" , $"Item {i}" , this . GroupName ); } } }`
2. Call the `MakeProfileAction` method in the class constructor: `this . MakeProfileAction ( "list;Select parameter:" );`
3. Add code to execute the command. In this example, it is as simple as changing the plugin status to show the selected parameter: `protected override void RunCommand ( String actionParameter ) => this . Plugin . OnPluginStatusChanged ( PluginStatus . Warning , actionParameter );`

### Create profile action ("tree")

As an example, we will implement a profile action that starts Windows Settings applications.

Windows Settings applications are grouped in categories, so UI should show two levels of combo boxes: for categories and for applications within a selected category.

1. In the plugin project, create a class based on either `PluginDynamicCommand` or `PluginDynamicAdjustment` base class.
2. Call the `MakeProfileAction` method in the class constructor: `this . MakeProfileAction ( "tree" );`
3. Override `GetProfileActionData` method to return tree data. `protected override PluginProfileActionData GetProfileActionData () { // create tree data var tree = new PluginProfileActionTree ( "Select Windows Settings Application" ); // describe levels tree . AddLevel ( "Category" ); tree . AddLevel ( "Application" ); // add data tree var categoryNames = this . _applications . Values . Select ( a => a . CategoryName ). Distinct (); foreach ( var categoryName in categoryNames ) { var node = tree . Root . AddNode ( categoryName ); var items = this . _applications . Values . Where ( a => a . CategoryName . EqualsNoCase ( categoryName )); foreach ( var item in items ) { node . AddItem ( item . ApplicationUri , item . ApplicationName , null ); } } // return tree data return tree ; }`
4. Define display names for each parameter: `protected override String GetCommandDisplayName ( String actionParameter , PluginImageSize imageSize ) => this . _applications . TryGetValue ( actionParameter , out var application ) ? application . ApplicationName : null ;`
5. Add code to execute the command. In this example, it is as simple as starting the application URI: `protected override void RunCommand ( String actionParameter ) => Process . Start ( actionParameter );`