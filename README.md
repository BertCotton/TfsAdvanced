# TfsAdvanced
##!Important Note!
Due to the tight integration of this app with VSTS, I converted it into a VSTS extension that does the same work but with less hassle. You can check out the source for it in the TfsAdvancedExt folder. To install the extension from the marketplace, please go here: [TfsAdvanced Active Pull Request Extension](https://marketplace.visualstudio.com/items?itemName=sierpinski.blitz-allpulls-extension)

##What is it?
This repository contains a webapp for getting TFS pull requests in a queue. This allows users to review open pull requests and click the link to go to TFS and work the pull request.

![alt Screenshot](https://github.com/sierpinski/TfsAdvanced/blob/master/kylesgithub.png)

##Details
This is for TFS 2015. This webapp is a little site that shows all of the open pull requests in a project. The approver can then follow a link into TFS and work the pull request.
The app uses .net core.
The app also uses Windows authentication. If you would like to add the ability use a token, please branch this and make a pull request.

##Why did I make it?
Our state's mail server went down and pull requests fell through the cracks. We also had some pull requests which dropped off the radar before they were completed. I just wanted a solution to see all of the pull requests across hundreds of repo.'s. So, I made one!

##How to make it work for your TFS system:
1. Obviously grab the code!
2. Then, change the BaseTFSAddress to your tfs address. For example, if your tfs is hosted at "https://www.ourtfs.com/tfs" then that is your base addess.
3. Next, change the TfsProject to be your project. So, if your project is "OurProject" then set that string.
4. Finally, the app is ready to go. It will construct address calls from your constants, so make sure you created them just like I did. (ie., the app will get all your repos by constructing the string "https://www.ourtfs.com/tfs/OurProject/_apis/git/repositories?api=1.0")
