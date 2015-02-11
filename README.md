In order to get the repository on your machine:

Install Git
http://git-scm.com/download/win


I strongly recommend checking the box which sais: integrate in command prompt or windows console. the you can simply open a command prompt and use git commands from it.

the first commands I suggest doing are:

git config --global user.name "your username"

git config --global user.password "your password"

git config --global user.email "your email address"

Then you can move on to getting the repository on your computer. To do that you use:

git clone "repository url"

in this case, for this repository, the clone command is:

git clone https://github.com/abiaco/random_BCI.git

that will create in the folder you are in with the command prompt a directory called random_BCI which will contain all the files on the folder

In order to download any changes that wil be made to the repository and keep it up to date, you need to simply use:

git pull origin master.

if you modify any files or add new ones to the folder, and want to upload them to the repository, you first need to let git know that you added changes:

git add .

(note the dot(.) at the end of the command)

then, once you told it to add all changes, you need to commit the change:

git commit -m "commit message"

in the commit message you basically should write what changes you made overall, but it is not at all essential.

once you committed the change, you need to push it to the server:

git push origin master

it will ask you for your username and password for github.

Another useful command is 

git status

That will show you the status of your current repository (as in, if you have added any new files and/or modified any existing ones that you did not commit and push, if there are any new changes on the repository online which you do not have yet (with a message like "your branch is behind origin/master by 1 commit") etc.).

if you are completely up to date with everything and there are no changes to commit / files modified that have not been added / committed, the message from git status will be "up to date with origin/master. nothing to commit".

That should be enough to pretty much get you going with using git!

For more information, use the git manual : http://git-scm.com/documentation
and, of course, google!

Have fun! feel free to experiment and play around with it, if anything goes wrong i'll help you out :)

