# game-kronatic_game
PS7 is the Networking Library, it was just named that for an assignments purpose
Repo for Tank Wars Game
developed by gabe bautista and blake vandyken

GAME CONTROLS:
W: Move up
S: Move down
D: Move right
A: Move left
LEFT CLICK: shoot
RIGH CLICK: use power up

TANK WARS SERVER DEFUALT FEATURES INCLUDED:
	-Tanks can shoot
	-Tanks can respawn
	-Tanks can gain points by destroying other tanks
	-Tanks can pick up power ups and shoot laser beams with the power up
	-Tanks can connect to the server and disconnect saftley
	-Tanks will loop around the map when they go outside of the map boundries
	-Server recieves commands from connected tanks and updates the world
	-Server detects collisions between walls, tanks, bullets, lasers, and acts accordingly
	-Server handles new connects and disconnects without server performance or crashes	

SETTINGS XML THAT CAN CHANGE:
	-Universe Size: Changes default map size | default: 2000
	-MSPerFrame: changes frame rate | default: 
	-FramePerShot:changes how often tanks can shoot | default: 80
	-RespawnRate:changes how fast tanks respawn | default: 300
	-Engine Strength: speed of tanks | default: 3.0
	-Max Power Ups: max power ups allowed on the map at one time | default: 2
	-Max Power Up Delay: respawn delay of creating new power ups | default: 1650
	-Starting Hit Points: starting hp of tanks | default: 3.0
	-Wall: where walls are spawned in on the map | default: border and other blocks on the map

OUR DESIGN DECISIONS:
	We decieded to implement all of these features so that the server is fully customizable from XML settings which allows quicker debugging, easier grading, and better code editing and readablitiy