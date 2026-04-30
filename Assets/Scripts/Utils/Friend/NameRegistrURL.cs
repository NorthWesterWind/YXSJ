using static Utils.LoginUtil;
public class FriendsURL
{
    public static string SearchFriendsUrl = "http://game.zikunhh.com/php/api.php?s=Friend.searchStrangers&app_name=";
    public static string AddFriendsUrl= "http://game.zikunhh.com/php/api.php?s=Friend.sendRequest&app_name=";
    public static string FriendRequestUrl = "http://game.zikunhh.com/php/api.php?s=Friend.getRequestList&app_name=" ;
    public static string ConSentFriendUrl = "http://game.zikunhh.com/php/api.php?s=Friend.handleRequest&app_name=";
    public static string FrinedsListUrl = "http://game.zikunhh.com/php/api.php?s=Friend.getFriendList&app_name=";
    public static string DeleteFrinedsUrl = "http://game.zikunhh.com/php/api.php?s=Friend.deleteFriend&app_name=";
    public static string GetUnreadCountUrl = "http://game.zikunhh.com/php/api.php?s=Chat.getUnreadCount&app_name="; 
     public static string GetStrangersUrl = "http://game.zikunhh.com/php/friend.php?app_name=";
     public static string SearchFriendedUrl = "http://game.zikunhh.com/php/api.php?s=Friend.searchFriends&app_name=";

    
}
public class ChatUrl
{
    public static string Send= "http://game.zikunhh.com/php/api.php?s=Chat.sendMessage&app_name=";
    public static string ChatrecordrequestUrl = "http://game.zikunhh.com/php/api.php?s=Chat.getMessages&app_name=";
    public static string BlockedWordUrl = "http://game.zikunhh.com/php/blocked.php?action=check";
}



public class FactoryUrl
{
    public static string Factory(string url)
    {
        return url+ "Yjsj";
    }
}