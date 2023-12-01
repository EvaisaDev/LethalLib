# LethalLib  
**A library for adding new content to Lethal Company, mainly for personal use.**
  
Currently includes:   
- Custom Scrap Item API  
- Custom Shop Item API  
- Unlockables API  
- Custom Enemy API  
- Network Prefab API  

# Recent Changes 
   
- Fixed issues with Unlockables API  
	- Non ship upgrades were getting added to ship upgrades list.  
	- Unlockables were getting readded every time a save was loaded.
- Fixed issue with items API  
	- Items with the same name were allowed to get registered multiple times.  
- Fixed custom shop items not saving/loading properly.
