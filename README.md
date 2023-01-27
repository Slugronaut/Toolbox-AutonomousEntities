# Toolbox-AutonomousEntities
System for creating boundries and roots for individual in-game entities as expressed in the GameObject hierarchy.

If you've ever had issues where you create seperate GameObjects that represent different in-game entities but were forces to parent them one reason or another, this system makes it easy to logically keep them seperate. It allows for easily components within a given entity without accidentally dipping into parent entities as well as caching common results for quick acces in the future regardless of how complicated or lareg your GameObject entity hierarchies may get.

Dependencies:  
[com.postegames.messagedispatcher](https://github.com/Slugronaut/Toolbox-MessageDispatch)  
[com.postegames.collections](https://github.com/Slugronaut/Toolbox-Collections)  
[com.postegames.typehelper](https://github.com/Slugronaut/Toolbox-TypeHelper)  
