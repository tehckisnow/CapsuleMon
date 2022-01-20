
    /*


TODO:

Glitches:

    sometimes when nicknaming a mon, text gets messed up ???
        multiple dialog coroutines being called at once
            GameController.Instance.WhenDialogClose()

    stepping into trainerFOV adjacent to trainer, trainer may rotate to face wrong direction
        this seems to be a result of the Character.Move() algorithm determining the shortest distance to an adjacent tile

    multiple evolution moves will leave the game in the wrong state

Features:

    sound effects/music

    flesh out all animations

    a way to see move descriptions would be nice

    reorder moves would be nice, too

    select or customize character?
    
    be able to reorder mons in mon storage

    automatically include trainer(/npc) name in certain dialogs

    HM moves can be used outside of battle
        CUT can temporarily remove certain small trees

    Opponent Scaler: attach to a trainer and it adjusts level of their mons based on level of player's mons (or base it on player's performance instead of level)

    support using console and cheats in battle

    --------------------------------------

    mon friendship levels

    interaction that gives money?

    one-time purchases from mart? make them savable? limited quantity of stock?

    items
        escape items
            pokedoll
        valuable/exchangable items
            gold nugget
                value
        flute
            gain an affect without consuming the item
                some flutes can be used in battle to remove an effect like sleep (pokeflute)
                some flutes can be used out of battle to perform an action (like adjusting encounter rate)
        fossils
            can be "revived" (traded at certain events) for special prehistoric mons
        repel
            repel (wild mons with a level lower than 1st member of player's party will not appear for next 100 steps)
            super repel (200 steps)
            max repel (250 steps)
        held items
            (many types)
        Battle Items
            X attack
            X defense
            ...
            Dire Hit
            Guard Spec
        vitamins
            hp up
            protein (attack)
            iron (defense)
            calcium (spatt)
            zinc (spdef)
            carbos (speed)
            pp up
            pp max
        herbal medicine( healing effect at cost of mon's friendship value)
            heal powder (heals all status problems)
            energy powder (heals 60hp)
            energy root (heals 120hp)
            revival herb (revives mon with full hp)

        item storage

        breeding

        daycare

Completed:

    //item that gives money?
        
    //Key items

    //tut should mention "cancel" opens menu

    //when enemy uses a status move on self, the playerHit animation is still played
        //(same when player uses status move on self, enemyHit animation is played)

    //trainer will walk diagonally to reach the player (is check for trainerbattle happenning too early?)

    //SpriteFader is broken (used in BattleSystem.ThrowCapsule())

    //Saving system in PlayerController is not setting player facing in character for some reason

    //capsules are not always being deducted from their count after being used

    //when a trainer's facing is changed in the inspector, their FOV does not rotate correctly when set to UP
    
    //Player is weirdly offset from the grid.  Has to do with Character.OffsetY and SetPositionAndSnapToTile() in Awake()

    //NPCs are weirdly offset vertically.  Has to do with SetPositionAndSnapToTile() in Awake();

    //problem with healing party

    // quest pickup revive is not persistent if the scene it is in is unloaded before the game is saved
    //     Secondary save system to temporarily handle state
    //     OR use secondary save file instead?
    
    //confirmation menu may need to unsub from events upon closing (check after implementing a second confirm menu)

    //nickname menu sometimes reopens (prof)

    //can activate dialog from one additional square away downward

    //get stuck on item screen when try to use an item in no battle when there are no items

    //confirm that moves like growl are adjusting attack correctly and attack is being factored in correctly

    //confirm money is correctly loading

    //confirm that 

    //ensure that player starts with a default warpPoint

    //save/load player NAME and muns

    //error if selecting "no mon" on monparty screen

    //-hide trainer sprite after trainerBattle (or at beginning of wildMonBattle) (I think this is fixed)
    
    //-battlescreen dialog overlaps choices (I think this is fixed)

    //set state correctly after giving mon a nick

    //show name and muns at load

    //load should be disabled if there is no load data

    //facing is not being properly restored from startmenu load

    //being able to skip dialog typing would be nice
    //also skip in battle dialog

    //being able to set the player's name would be nice

    //yes/no confirmation menu would be nice

    //yes/no confirmation menu : should there be a default action for NoAction?

    //give mons nicknames

    // menu
    //     mons
    //         info
    //         reorder
    //         release

    //reorder menu

    //release

    //trainer enterBattle animation

    //obtain 7th mon...

    // partyscreen HP bars are too long? wtf
    //     every time I load they get longer
    
    //pikachu keeps trying to evolve after I catch pokemon (use pokeball?)

    //prevent last mon from being stored or released

    //monstorage system
        //check if mon has been initialized before (set bool value upon init?)
        //upon opening system a second time, no option is selected
        //can select an option out of range (could this have to do with having taken a mon?)
        //dialog is behind monstorage window
        //null condition for if monstorage is empty
        //scrolling
        //reorder mons
        //confirmation menu to withdraw

    //Transfer mon to storage sys
        //confirmation menu to deposit

    //after using rarecandy, charmander automatically stopped evolving
    //inventoryUI selector is appearing in weird places
        //index appears to be being reset upon reopening the menu, but UI isn't

    //monparty UI snak lv20 is displayed on 2 lines

    //Confirm that glare only works 75% of the time

    //confirm pokemon learn moves after using rarecandy

    //Inventory.UseItem() success or failure changes outcome of evolution
    
    //only remove evolution stone if evolution is not skipped
        //(or prevent skipping evolution when using a stone?)

    //Press X during the message; "mon.Name is evolving"

    //nickname changes upon evolution if matches species name
        //if(mon.Base.Name.ToLower() == mon.Name.ToLower()){}    ?
    
    // after using last item in inventory (evolution stone) and evolution takes place, 
    //     after evolution screen closes and return to inventory, UI still shows evolution stone
    //         update UI

    //maximum nickname length

    //TOO MUCH XP
    
    //Only show battle reward notification if battlereward is > 0

    //prevent action selection after mon faints
    
    // check; 
    //     mon evolves in battle
    //         CheckForEvolutionMove
    //             battle continues
    //                 are new moves listed in move list or does update need to be called?

    //paint outside of monmart or something

    // Scripts modified to use new DialogQueue:
    //     Pickup
    //     GymLeaderReward
    //     PlayerController
    //     GameController
    //     EvolutionManager
    //     MonConsole
    //     ItemGiver
    //     InventoryUI
    //     Mon
    //     MonGiver
    //     MonParty
    //     MonStorage
    //     Quest
    //     NicknameMenu
    //     Nicknamer
    //     ShopUI
    //     BattleSystem
    //     Sign
    //     NPCController
    //     StoryItem
    //     TrainerController

    //re-enable storyevent blocking route1

    //disable cheats by default

    //lower pidge's xp yield

    //place computer storage

    //place nicknamer

    //ending in menu state when using Normal badge

    //using a rare candy leaves the mon with a full XP bar

    //finish quest confirmation prompt is coming up when finishing a quest on the same interaction as starting it

    //cheat to heal mon/party

    //saveGame isn't restoring lost hp

    //items in cheat menu are clustered and overlapping

    //poison hurts mons outside of battle.  if all mon faint, tp to last warppoint

    //ensure that lastWarppoint is being saved

    //ensure that every place a mon can be healed or cured of poison, mon.isFainted is set to false
        //use revive in itemMenu and in battle
    
    //fix front door collision

    //Ensure everything works from titleScreen

    //tutorial controls

    //nurse in Town2 didn't heal my mon

    //more money

    //ensure confirmation message on completing quests is optional (toggle bool)

    //where do you get the bike from?
    //customActionObject to get bike quest

    //Dialog system Queue

    //lv 100 is 2 lines on monInfo screen

    //mon.GetLearnableMovesAtCurrentLevel()
    //multiple learnable moves on the same level overlap
    //message text is colliding

    //charmander trying to evolve after battle (before reaching level 16)

    //option to cancel evolution?

    // mon learn moves upon evolution?
    //     choose move to forget outsideOfBattle menu

    //set nickname upon evolution?

    //Name rater (to set new nick)

    //nickname menu: previous name should not use Base

    //see status from partymenu

    //(maybe not)  show XP bar in partyMenu would be nice

    //selecting a mon from partyscreen to see infoscreen showing frontImage, moves, xp, Base.name and nickname, etc. would be nice

    //error on monInfoScreen

    //show PP of each move on infoscreen?

    //when losing a battle, fade to black and teleport to last pokemon center
    
    //Optional:

        //Store Interacton/UI to purchase items

        //bike
            //add to inventory
            //use in inventory
            //only use outside
            //message from prof if use inside
            //block doors when on bike
    
    //content
        //starter mon
            //starter mons' moves
            //starter mon evolutions
                //starter mon evolutions' moves
        //other mons
            //other mons' moves
            //other monss evolutions
                //other mons' evolutions' moves
        //items
            //gym badge (open access to new area?)
        //trainers
            //trainers' teams
        //gym
            //building layout/theme/type/puzzle
            //gym leader
                //leader's team
                //reward (a new TM?)
            //initiate congratulatory dialog (cutscene) right after battle has ended (if won)

    //Opening
    */
