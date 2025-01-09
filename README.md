# Prototype-1
Initial prototype of mvc project

How to use our repository:

Clone into feature branch
-> Find the repo "Prototype 1"
-> Click on the "main" branch drop down list
-> Click "feature"
-> Click the green "Code" drop down list
-> Click SSH
-> Copy the SSH key for the feature branch

-> Create an empty folder called "Tomorrows-voices" on your desktop
-> Open git bash and type "cd ~", click enter
-> Type "cd Desktop/Tomorrows-voices/", click enter
-> Type "git clone (paste your SSH key here)", click enter
-> Finally type cd and then tab to open the new folder you cloned

IMPORTANT:

Always type "git fetch" before pulling, commiting, pushing or checking the status.

Submitting changes to github:
Once you have made your changes, open your git bash, ensure you are in the right directory and on feature branch.
Do a git fetch, then once you have downloaded the changes, do a git pull.

Next, type "git status" and it will show you all the files you have made changes to.
Type git add example.html, and only add pages that you actually made changes to in order to avoid code getting lost

Once you have added your files, run another git status to ensure you aren't missing any.

Run git fetch
Then type "git commit -m "in the quotes, add a descriptive message about what you did, all users will be able to read it"  "
Finally, run one more git fetch
Then type "git push origin feature" to ensure changes are not pushed to main.

Main branch will be updated once project is in a solid state and tested.

