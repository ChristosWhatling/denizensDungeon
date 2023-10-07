Module Module1

    'debug settings
    Const displayStats As Boolean = False 'whether to display the player's stats
    Const displayPlayerCoords As Boolean = False
    Const displayEnemyMovements As Boolean = False

    Const NoOfStats As Integer = 11
    Const MaxPlayerCount As Integer = 1
    Const NoOfKindAbstrata As Integer = 12
    Const InitialStatVariance As Integer = 0 'number of times stats have a change to be increased or decreased
    '0 ---> 5
    '1 ---> 4-6
    '2 ---> 3-7
    '3 ---> 2-8
    '4 ---> 1-9
    '5+ --> 0-10

    Public playerarray(MaxPlayerCount) As Player
    Public enemyList As New List(Of Enemy)
    Public NPCHasSpawned(8) As Boolean
    Public theDungeon As New List(Of DungeonFloor)

    Public currentlyPlayingTrack As String = ""
    Public isMusicMuted As Boolean = False
    Public menuMusicOnLaunch As Boolean = True

    'Entire Party Variables
    Public gristCache As Integer = 0
    Public landXPos As Integer = 0
    Public landYPos As Integer = 0
    Public dungeonXPos As Integer = 0
    Public dungeonYPos As Integer = 0

    Public Class Player

        'Bio
        Public playerId As Integer
        Public firstName As String
        Public lastName As String
        Public chumhandle As String
        Public chumhandleAbrv As String
        Public ageYears As Double
        Public ageSweeps As Double
        Public gender As String
        Public lunarSway As Integer
        Public classTitle As Integer
        Public aspectTitle As Integer
        Public species As Integer
        Public typingQuirk(7) As Integer
        Public typingQuirkReplacements(6) As String
        Public kindAbstratus(16) As Integer
        Public kindAbstratusState(16) As Integer

        'Species Specific

        'Human
        Public eyeColour As String


        'Stats
        Public stats(11) As Integer
        Public maxHealth As Integer
        Public initialMaxHealth As Integer
        Public currentHealth As Integer
        Public alive As Boolean
        Public statusEffects(512) As Integer
        Public temporalStatusEffects(512) As Integer
        Public currentQu As Integer
        Public maxQu As Integer
        Public initialMaxQu As Integer
        Public QuStartTurnGain As Integer
        Public hasInfiniteMaxQu As Boolean
        Public initialHandSize As Integer 'handSize at start of combat
        Public currentHandSize As Integer 'current ^

        'Inventory
        Public equippedWeapon As Integer 'points to an item in inventory
        Public equippedPassiveSlots(256) As Integer 'points to an item in inventory

        'Action Deck
        Public initialRegistry As New List(Of Integer)
        Public actionRegistry As New List(Of Integer)
        Public actionDeathbed As New List(Of Integer)
        Public actionFurthestRing As New List(Of Integer)
        Public playerHand As New List(Of Integer)

        'Combat Variables
        Public cardDrawDouble As Integer
        Public nextActionAdditionalResolve As Integer
        Public accumulateList As New List(Of Integer)
        Public nextLuckAdvantage As Integer
        Public extraTurnAfterCurrentTurn As Integer

        'Move to Run generator (all players share them)
        Public inventory(512) As Integer 'only 0-15 is used normally, but items can extend the inventory
        Public kiddieCamperHandysash(16) As Boolean 'each boolean refers to a badge the player has or doesn't have

        Public Sub New()
            Me.lunarSway = Math.Ceiling(Rnd() * 2)
            Me.classTitle = Math.Ceiling(Rnd() * 12)
            Me.aspectTitle = Math.Ceiling(Rnd() * 12)
            Me.kindAbstratus(1) = Math.Ceiling(Rnd() * NoOfKindAbstrata)
            Me.kindAbstratusState(1) = 0
            For x = 2 To 16
                Me.kindAbstratus(x) = 0
                Me.kindAbstratusState(x) = 0
            Next
            GenderGen()
            ChumHandleAbrvGen()

            'Combat Stats
            Me.initialMaxHealth = 75
            Me.maxHealth = Me.initialMaxHealth
            Me.currentHealth = Me.maxHealth
            Me.alive = True
            Me.maxQu = 2
            Me.initialMaxQu = 2
            Me.currentQu = 2
            Me.initialHandSize = 5
            Me.currentHandSize = Me.initialHandSize
            Me.hasInfiniteMaxQu = False

            gristCache = 0
        End Sub

        'Stats

        Public Sub StatGen()
            Dim randno As Integer
            For x = 1 To NoOfStats
                Me.stats(x) = 5
                For x2 = 1 To InitialStatVariance
                    randno = Math.Ceiling(Rnd() * 4)
                    If randno = 1 Then
                        Me.stats(x) += 1
                    ElseIf randno = 4 Then
                        Me.stats(x) += -1
                    End If
                Next
            Next

            'Lunar Sway Stat Bonuses
            If Me.lunarSway = 1 Then 'Prospit
                AddStats(8, 2)
                AddStats(10, 1)
                AddStats(9, 1)
                AddStats(4, -1)
            ElseIf Me.lunarSway = 2 Then 'Derse
                AddStats(6, 1)
                AddStats(7, 1)
                AddStats(3, 1)
            End If

            'Aspect Stat Bonuses
            Select Case Me.aspectTitle
                Case 1 'Time
                    AddStats(2, 2)
                    AddStats(1, -1)
                Case 2 'Space
                    AddStats(1, 2)
                    AddStats(5, -1)
                Case 3 'Heart
                    AddStats(10, -1)
                    AddStats(6, 1)
                    AddStats(1, 1)
                Case 4 'Mind
                    AddStats(6, 1)
                Case 5 'Hope
                    AddStats(10, 2)
                    AddStats(9, 1)
                    AddStats(6, -1)
                    AddStats(1, -1)
                Case 6 'Rage
                    AddStats(10, -2)
                    AddStats(1, 1)
                    AddStats(3, 1)
                    AddStats(9, 1)
                Case 7 'Light
                    AddStats(11, 3)
                    AddStats(4, -2)
                Case 8 'Void
                    AddStats(4, 2)
                    AddStats(5, -1)
                Case 9 'Breath
                    AddStats(8, 1)
                    AddStats(10, 1)
                    AddStats(4, -1)
                Case 10 'Blood
                    AddStats(7, 2)
                    AddStats(9, 1)
                    AddStats(1, -1)
                    AddStats(4, -1)
                Case 11 'Life
                    AddStats(3, 1)
                    AddStats(5, 2)
                    AddStats(8, -1)
                    AddStats(2, -1)
                Case 12 'Doom
                    AddStats(11, -1)
                    AddStats(7, -1)
                    AddStats(5, 1)
                    AddStats(3, 2)
            End Select

            'Class Stat Bonuses
            Select Case Me.classTitle
                Case 1 'Heir
                    AddToHighestStats(1)
                    AddStats(10, 1)
                Case 2 'Witch
                    AddToHighestStats(1)
                    AddStats(8, 1)
                Case 3 'Page
                    AddStats(7, 3)
                    AddStats(10, 2)
                    For x = 1 To NoOfStats
                        AddStats(x, -1)
                    Next
                Case 4 'Knight
                    AddStats(1, 1)
                    AddStats(2, 1)
                Case 5 'Seer
                    AddStats(6, 1)
                Case 6 'Mage
                    AddStats(6, 3)
                    AddStats(7, -1)
                    AddStats(10, -1)
                Case 7 'Sylph
                    AddStats(5, 2)
                Case 8 'Maid
                    AddToLowestStats(-1)
                    AddStats(5, 3)
                    AddStats(1, 1)
                Case 9 'Thief
                    'undecided
                Case 10 'Rogue
                    'undecided
                Case 11 'Prince
                    SetHighestStats(8)
                    SetLowestStats(2)
                    ReplaceAllStatsOfValue(8, -1)
                    ReplaceAllStatsOfValue(2, 8)
                    ReplaceAllStatsOfValue(-1, 2)
                Case 12 'Bard
                    SetHighestStats(8)
                    SetLowestStats(2)
            End Select

            MaxMinStats(8, 2)

        End Sub

        Public Function GetHighestStatValue()
            Dim highestValue As Integer = 0
            For x = 1 To NoOfStats
                If Me.stats(x) > highestValue Then
                    highestValue = Me.stats(x)
                End If
            Next
            Return highestValue
        End Function

        Public Function GetLowestStatValue()
            Dim lowestValue As Integer = 10
            For x = 1 To NoOfStats
                If Me.stats(x) < lowestValue Then
                    lowestValue = Me.stats(x)
                End If
            Next
            Return lowestValue
        End Function

        Public Sub AddToHighestStats(addAmount As Integer)
            Dim highestValue As Integer = GetHighestStatValue()
            For x = 1 To NoOfStats
                If Me.stats(x) = highestValue Then
                    Me.stats(x) += addAmount
                End If
            Next
        End Sub

        Public Sub SetHighestStats(setAmount As Integer)
            Dim highestValue As Integer = GetHighestStatValue()
            For x = 1 To NoOfStats
                If Me.stats(x) = highestValue Then
                    Me.stats(x) = setAmount
                End If
            Next
        End Sub

        Public Sub AddToLowestStats(addAmount As Integer)
            Dim lowestValue As Integer = GetLowestStatValue()
            For x = 1 To NoOfStats
                If Me.stats(x) = lowestValue Then
                    Me.stats(x) += addAmount
                End If
            Next
        End Sub

        Public Sub SetLowestStats(setAmount As Integer)
            Dim lowestValue As Integer = GetLowestStatValue()
            For x = 1 To NoOfStats
                If Me.stats(x) = lowestValue Then
                    Me.stats(x) = setAmount
                End If
            Next
        End Sub

        Public Sub ReplaceAllStatsOfValue(valueToReplace As Integer, replacementValue As Integer)
            For x = 1 To NoOfStats
                If Me.stats(x) = valueToReplace Then
                    Me.stats(x) = replacementValue
                End If
            Next
        End Sub

        Public Sub SetStats(statToSet As Integer, setValue As Integer)
            Me.stats(statToSet) = setValue
        End Sub

        Public Sub AddStats(statToSet As Integer, addValue As Integer)
            Me.stats(statToSet) += addValue
        End Sub

        Public Sub MaxMinStats(maxStat As Integer, minStat As Integer)
            For x = 1 To NoOfStats
                If Me.stats(x) > maxStat Then
                    Me.stats(x) = maxStat
                ElseIf Me.stats(x) < minStat Then
                    Me.stats(x) = minStat
                End If
            Next
        End Sub

        Public Sub RoundTotalStats(roundTo As Integer)
            Dim statTotal As Integer = 0
            For x = 1 To NoOfStats 'Add up total stats
                statTotal += Me.stats(x)
            Next

            If statTotal = roundTo Then 'if already equal, then end sub
                Exit Sub
            ElseIf statTotal > roundTo Then 'if larger than target
                Dim largestStatValue As Integer
                Dim largestStat As Integer
                Do 'removes 1 from the highest stat until meets target stat total, priority to stats later in the array
                    largestStatValue = 0
                    largestStat = 0
                    For x = 1 To NoOfStats
                        If Me.stats(x) > largestStatValue Then
                            largestStatValue = Me.stats(x)
                            largestStat = x
                        End If
                    Next
                    Me.stats(largestStat) -= 1
                    statTotal -= 1
                Loop Until statTotal <= roundTo
            Else 'if smaller than target
                Dim lowestStatValue As Integer
                Dim lowestStat As Integer
                Do 'adds 1 to the highest stat until meets target stat total, priority to stats earlier in the array
                    lowestStatValue = 8 'stats will never be greater than 8 right after generation
                    lowestStat = 0
                    For x = NoOfStats To 1 Step -1
                        If Me.stats(x) < lowestStatValue Then
                            lowestStatValue = Me.stats(x)
                            lowestStat = x
                        End If
                    Next
                    Me.stats(lowestStat) += 1
                    statTotal += 1
                Loop Until statTotal >= roundTo
            End If
        End Sub

        Public Sub ThiefStatSetup()
            For x = 1 To MaxPlayerCount
                If x <> Me.playerId Then
                    Dim highestValue As Integer = playerarray(x).GetHighestStatValue()
                    For x2 = 1 To NoOfStats
                        If playerarray(x).stats(x2) = highestValue Then
                            Me.stats(x2) += 1
                        End If
                    Next
                End If
            Next
        End Sub

        Public Sub RogueStatSetup()
            Dim highestValue As Integer = GetHighestStatValue()
            For x = 1 To MaxPlayerCount
                If x <> Me.playerId Then
                    For x2 = 1 To NoOfStats
                        If Me.stats(x2) = highestValue Then
                            playerarray(x).stats(x2) += 1
                        End If
                    Next
                End If
            Next
        End Sub

        'General Generation

        Public Sub GenderGen()
            Dim i As Integer = Math.Ceiling(Rnd() * 2) 'number from 1 to 2 chosen
            If i = 1 Then
                Me.gender = "Female"
            Else
                Me.gender = "Male"
            End If
        End Sub

        Public Sub ChumHandleAbrvGen()
            Dim i As Integer = Math.Ceiling(Rnd() * 26) - 1
            Dim char1 As Char = Chr((Asc("A") + i))
            i = Math.Ceiling(Rnd() * 26) - 1
            Dim char2 As Char = Chr((Asc("A") + i))
            Me.chumhandleAbrv = char1 + char2

            ChumhandleGen(char1, char2)
        End Sub

        Public Sub MakeSelfUnique()
            Dim isUnique As Boolean = True
            Me.lunarSway = Math.Ceiling(Rnd() * 2)

            Do
                isUnique = True
                Me.classTitle = Math.Ceiling(Rnd() * 12)
                For i = 1 To MaxPlayerCount
                    If Me.classTitle = playerarray(i).classTitle Then
                        isUnique = False
                    End If
                Next
            Loop Until isUnique = True

            Do
                isUnique = True
                Me.aspectTitle = Math.Ceiling(Rnd() * 12)
                For i = 1 To MaxPlayerCount
                    If Me.aspectTitle = playerarray(i).aspectTitle Then
                        isUnique = False
                    End If
                Next
            Loop Until isUnique = True

            Do
                isUnique = True
                Me.kindAbstratus(1) = Math.Ceiling(Rnd() * NoOfKindAbstrata)
                For i = 1 To MaxPlayerCount
                    If Me.kindAbstratus(1) = playerarray(i).kindAbstratus(1) Then
                        isUnique = False
                    End If
                Next
            Loop Until isUnique = True

            Do
                isUnique = True
                Me.eyeColour = Math.Ceiling(Rnd() * 12)
                For i = 1 To MaxPlayerCount
                    If Me.eyeColour = playerarray(i).eyeColour Then
                        isUnique = False
                    End If
                Next
            Loop Until isUnique = True

            Do 'Unique Chumhandle abbreviations
                isUnique = True
                Dim i As Integer = Math.Ceiling(Rnd() * 26) - 1
                Dim char1 As Char = Chr((Asc("A") + i))
                i = Math.Ceiling(Rnd() * 26) - 1
                Dim char2 As Char = Chr((Asc("A") + i))
                Me.chumhandleAbrv = char1 + char2
                For i = 1 To MaxPlayerCount
                    If Me.chumhandle = playerarray(i).chumhandleAbrv Then
                        isUnique = False
                    End If
                Next
                ChumhandleGen(char1, char2)
            Loop Until isUnique = True

            StatGen() 'need to set stats again due to new classpect and lunarsway
        End Sub 'UNUSED

        Public Sub ChumhandleGen(char1 As Char, char2 As Char)
            char1 = LCase(char1)
            Dim fullline As String = ""
            Dim chumhandle1sthalf As New List(Of String)
            Dim chumhandle2ndhalf As New List(Of String)
            Dim counter1 As Integer = 0
            Dim counter2 As Integer = 0
            FileOpen(1, CurDir() & "\data\pesterchumhandle1sthalf.txt", OpenMode.Input)
            FileOpen(2, CurDir() & "\data\pesterchumhandle2ndhalf.txt", OpenMode.Input)
            Do Until EOF(1)
                fullline = LineInput(1)
                If fullline.Substring(0, 1) = char1 Then
                    counter1 += 1
                    chumhandle1sthalf.Add(fullline)
                End If
            Loop
            Do Until EOF(2)
                fullline = LineInput(2)
                If fullline.Substring(0, 1) = char2 Then
                    counter2 += 1
                    chumhandle2ndhalf.Add(fullline)
                End If
            Loop
            Dim i1 As Integer = Math.Ceiling(Rnd() * counter1 - 1)
            Dim i2 As Integer = Math.Ceiling(Rnd() * counter2 - 1)
            FileClose(1)
            FileClose(2)
            Me.chumhandle = chumhandle1sthalf(i1) + chumhandle2ndhalf(i2)
        End Sub

        Public Sub WriteInfoIntoConsole()
            Console.WriteLine("-----------------------------------------------------")
            Console.WriteLine("Name: " & Space(42 - Len(Me.lastName)) & Me.firstName & " " & Me.lastName)
            Console.WriteLine("Chumhandle:" & Space(37 - Len(Me.chumhandle)) & Me.chumhandle & " [" & Me.chumhandleAbrv & "]")
            Console.WriteLine("Species: " & Space(39) & SpeciesIntToString(Me.species))
            Console.WriteLine("Gender: " & Space(45 - Len(Me.gender)) & Me.gender)
            Console.WriteLine("Age (Years): " & Space(40 - Len(Me.ageYears.ToString)) & Me.ageYears.ToString)
            Console.WriteLine("Age (Sweeps): " & Space(39 - Len(Me.ageSweeps.ToString)) & Me.ageSweeps.ToString)
            Console.WriteLine("")
            Console.WriteLine("Lunar Sway: " & Space(41 - Len(LunarSwayIntToString(Me.lunarSway))) & LunarSwayIntToString(Me.lunarSway))
            Console.WriteLine("Classpect: " & Space(38 - Len(ClassIntToString(Me.classTitle)) - Len(AspectIntToString(Me.aspectTitle))) & ClassIntToString(Me.classTitle) & " Of " & AspectIntToString(Me.aspectTitle))
            Console.WriteLine("Kind Abstrata: " & Space(38 - Len(KindAbstratusIntToString(Me.kindAbstratus(1), 0))) & KindAbstratusIntToString(Me.kindAbstratus(1), 0))
            Console.WriteLine("")
            For i = 1 To NoOfStats
                Console.WriteLine(StatName(i) & ": " & Space(50 - Len(StatName(i))) & Me.stats(i))
            Next
            Say("The quick brown fox jumps over the lazy dog.")
        End Sub 'UNUSED

        Public Sub WriteShortPlayerInfo()
            Console.WriteLine(Me.firstName & " " & Me.lastName & Space(30))
            Console.WriteLine(Me.chumhandle & Space(30))
            Console.WriteLine(ClassIntToString(Me.classTitle) & " Of " & AspectIntToString(Me.aspectTitle) & Space(30))
            BlankLine()
            Console.WriteLine("Health: " & Me.currentHealth & "/" & Me.maxHealth & Space(30))
            If displayStats Then
                For i = 1 To NoOfStats
                    If i = 3 Or i = 6 Or i = 9 Then
                        Console.WriteLine(StatNameAbrv(i) & ": " & Me.stats(i) & Space(30))
                    Else
                        Console.Write(StatNameAbrv(i) & ": " & Me.stats(i) & Space(5))
                    End If
                Next
            End If
            If hasInfiniteMaxQu = False Then
                Console.WriteLine("Qu: " & Me.currentQu & "/" & Me.maxQu & Space(30))
            Else
                Console.WriteLine("Qu: ∞/∞" & Space(30))
            End If
            BlankLine()

            Dim HasStatusEffectBeenShown As Boolean = False
            For x = 0 To 512
                If Me.statusEffects(x) > 0 Then
                    Console.WriteLine(ReadTextFile("\data\statuseffects.txt", x + 1) & ": " & Me.statusEffects(x) & Space(30))
                    HasStatusEffectBeenShown = True
                End If
            Next

            If HasStatusEffectBeenShown Then
                BlankLine()
            End If

            Console.WriteLine("Hand: " & Me.playerHand.Count & Space(30))
            Console.WriteLine("Registry: " & Me.actionRegistry.Count & Space(30))
            Console.WriteLine("Deathbed: " & Me.actionDeathbed.Count & Space(30))
            Console.WriteLine("Furthest Ring: " & Me.actionFurthestRing.Count & Space(30))
            Console.WriteLine("Total Actions: " & Me.playerHand.Count + Me.actionRegistry.Count + Me.actionDeathbed.Count + Me.actionFurthestRing.Count & Space(30))
        End Sub

        Public Sub DisplayEnemyInfo()
            For x = 0 To enemyList.Count - 1
                enemyList(x).CheckIfAlive()

                If enemyList(x).currentHP > enemyList(x).maxHP Then
                    enemyList(x).currentHP = enemyList(x).maxHP
                End If

                If enemyList(x).alive Then
                    Console.WriteLine(Space(30) & enemyList(x).name & Space(30))
                    Console.WriteLine(Space(30) & "HP: " & enemyList(x).currentHP & "/" & enemyList(x).maxHP & Space(30))
                    For y = 0 To 512
                        If enemyList(x).statusEffects(y) > 0 Then
                            Console.WriteLine(Space(30) & ReadTextFile("\data\statuseffects.txt", y + 1) & ": " & enemyList(x).statusEffects(y) & Space(30))
                        End If
                    Next
                    BlankLine()
                End If
            Next
        End Sub

        Public Sub ClearSelf()
            Me.firstName = "Null"
            Me.lastName = "McEmpty"
            Me.chumhandleAbrv = "VOID"
            Me.classTitle = 0
            Me.aspectTitle = 0
            Me.lunarSway = 0
            Me.kindAbstratus(1) = 0
        End Sub

        Public Sub Say(text As String)
            SetTextToEyeColour(Me.eyeColour)
            Console.WriteLine(Me.chumhandleAbrv & ": " & text)
            ResetTextColour()
        End Sub

        'Registry Generation
        Public Sub GenerateStartingRegistry()
            For x = 1 To 3
                AddActionToArea(0, -1)
            Next
            For x = 1 To 6
                AddActionToArea(1, -1)
            Next
            For x = 1 To 2
                AddActionToArea(120, -1)
                AddActionToArea(4, -1)
            Next
            AddActionToArea(Me.classTitle + 4, -1)
            AddActionToArea(Me.aspectTitle + 16, -1)
        End Sub

        'Species Specific Subs
        'Human
        Public Sub HumanGen()
            Me.species = 0
            HumanFirstNameGen()
            HumanLastNameGen()
            HumanAgeGen()
            HumanAppearanceGen()
        End Sub

        Public Sub HumanFirstNameGen()
            Dim fullline As String = ""
            Dim names As New List(Of String)
            Dim file As String
            Select Case Me.gender
                Case "Female"
                    file = "\data\humanfemalefirstnames.txt"
                Case "Male"
                    file = "\data\humanmalefirstnames.txt"
            End Select
            Dim counter As Integer = 0
            FileOpen(1, CurDir() & file, OpenMode.Input)
            Do Until EOF(1)
                counter += 1
                fullline = LineInput(1)
                names.Add(fullline)
            Loop
            Dim i As Integer = Math.Ceiling(Rnd() * counter - 1)
            FileClose(1)
            Me.firstName = names(i)
        End Sub

        Public Sub HumanLastNameGen()
            Dim fullline As String = ""
            Dim names As New List(Of String)
            Dim counter As Integer = 0
            FileOpen(1, CurDir() & "\data\humanlastnames.txt", OpenMode.Input)
            Do Until EOF(1)
                counter += 1
                fullline = LineInput(1)
                names.Add(fullline)
            Loop
            Dim i As Integer = Math.Ceiling(Rnd() * counter - 1)
            FileClose(1)
            Me.lastName = names(i)
        End Sub

        Public Sub HumanAgeGen()
            Me.ageYears = Math.Ceiling(Rnd() * 5) + 12 'number from 13 to 17 chosen
            Me.ageSweeps = ConvertYearsToSweeps(Me.ageYears)
        End Sub

        Public Sub HumanAppearanceGen()
            Me.eyeColour = Math.Ceiling(Rnd() * 12)
        End Sub

        Public Sub HumanTypingQuirkGen()
            Me.typingQuirk(1) = Math.Ceiling(Rnd() * 2)
            For x = 1 To 6
                Me.typingQuirk(x + 1) = 0
                Me.typingQuirkReplacements(x) = 0
            Next
        End Sub

        'Combat Subroutines

        'Legend
        'Areas:
        '
        '-1 = Initial Registry (Out of Combat Deck)
        '0 = Hand
        '1 = Action Registry
        '2 = Action Deathbed
        '3 = Action Furthest Ring

        'NO LONGER USED
        'Public Sub InitialiseTestRegistry(amount As Integer)
        'For x = 1 To amount
        'me.actionRegistry.Add(x - 1)
        'Next
        'End Sub

        Public Sub SortInitReg()
            Dim regCount As Integer = Me.initialRegistry.Count
            Dim tempList As New List(Of Integer)
            Dim lowestID As Integer

            For i = 0 To regCount - 1
                lowestID = 10000
                For x = 0 To Me.initialRegistry.Count - 1
                    If Me.initialRegistry(x) < lowestID Then
                        lowestID = Me.initialRegistry(x)
                    End If
                Next
                tempList.Add(lowestID)
                Me.initialRegistry.Remove(lowestID)
            Next
            Me.initialRegistry.Clear()

            For x = 0 To tempList.Count - 1
                Me.initialRegistry.Add(tempList(x))
            Next
        End Sub

        Public Sub ShuffleArea(areaToShuffle As Integer)
            Dim tempList1 As New List(Of Integer)
            Dim tempList2 As New List(Of Integer)
            tempList1.Clear()
            tempList2.Clear()

            Select Case areaToShuffle
                Case 0
                    For x = 0 To Me.playerHand.Count - 1
                        tempList1.Add(Me.playerHand(x))
                    Next
                Case 1
                    For x = 0 To Me.actionRegistry.Count - 1
                        tempList1.Add(Me.actionRegistry(x))
                    Next
                Case 2
                    For x = 0 To Me.actionDeathbed.Count - 1
                        tempList1.Add(Me.actionDeathbed(x))
                    Next
                Case 3
                    For x = 0 To Me.actionFurthestRing.Count - 1
                        tempList1.Add(Me.actionFurthestRing(x))
                    Next
            End Select

            Dim randNo As Integer

            For x = 0 To tempList1.Count - 1
                randNo = Math.Ceiling(Rnd() * tempList1.Count - 1)
                tempList2.Add(tempList1(randNo))
                tempList1.RemoveAt(randNo)
            Next

            Select Case areaToShuffle
                Case 0
                    Me.playerHand.Clear()
                    For x = 0 To tempList2.Count - 1
                        Me.playerHand.Add(tempList2(x))
                    Next
                Case 1
                    Me.actionRegistry.Clear()
                    For x = 0 To tempList2.Count - 1
                        Me.actionRegistry.Add(tempList2(x))
                    Next
                Case 2
                    Me.actionDeathbed.Clear()
                    For x = 0 To tempList2.Count - 1
                        Me.actionDeathbed.Add(tempList2(x))
                    Next
                Case 3
                    Me.actionFurthestRing.Clear()
                    For x = 0 To tempList2.Count - 1
                        Me.actionFurthestRing.Add(tempList2(x))
                    Next
            End Select
        End Sub

        Public Sub MoveAreaIntoAnotherArea(areaToMove As Integer, areaToMoveInto As Integer)
            Dim tempList As New List(Of Integer)

            If areaToMove = areaToMoveInto Then
                'do nothing
            Else
                Select Case areaToMove
                    Case -1
                        For x = 0 To Me.initialRegistry.Count - 1
                            tempList.Add(Me.initialRegistry(x))
                        Next
                        Me.initialRegistry.Clear()
                    Case 0
                        For x = 0 To Me.playerHand.Count - 1
                            tempList.Add(Me.playerHand(x))
                        Next
                        Me.playerHand.Clear()
                    Case 1
                        For x = 0 To Me.actionRegistry.Count - 1
                            tempList.Add(Me.actionRegistry(x))
                        Next
                        Me.actionRegistry.Clear()
                    Case 2
                        For x = 0 To Me.actionDeathbed.Count - 1
                            tempList.Add(Me.actionDeathbed(x))
                        Next
                        Me.actionDeathbed.Clear()
                    Case 3
                        For x = 0 To Me.actionFurthestRing.Count - 1
                            tempList.Add(Me.actionFurthestRing(x))
                        Next
                        Me.actionFurthestRing.Clear()
                End Select

                Select Case areaToMoveInto
                    Case -1
                        For x = 0 To tempList.Count - 1
                            Me.initialRegistry.Add(tempList(x))
                        Next
                    Case 0
                        For x = 0 To tempList.Count - 1
                            Me.playerHand.Add(tempList(x))
                        Next
                    Case 1
                        For x = 0 To tempList.Count - 1
                            Me.actionRegistry.Add(tempList(x))
                        Next
                    Case 2
                        For x = 0 To tempList.Count - 1
                            Me.actionDeathbed.Add(tempList(x))
                        Next
                    Case 3
                        For x = 0 To tempList.Count - 1
                            Me.actionFurthestRing.Add(tempList(x))
                        Next
                End Select
            End If
        End Sub

        Public Sub CopyAreaIntoAnotherArea(AreaToCopy As Integer, areaToCopyInto As Integer)
            Dim tempList As New List(Of Integer)

            Select Case AreaToCopy
                Case -1
                    For x = 0 To Me.initialRegistry.Count - 1
                        tempList.Add(Me.initialRegistry(x))
                    Next
                Case 0
                    For x = 0 To Me.playerHand.Count - 1
                        tempList.Add(Me.playerHand(x))
                    Next
                Case 1
                    For x = 0 To Me.actionRegistry.Count - 1
                        tempList.Add(Me.actionRegistry(x))
                    Next
                Case 2
                    For x = 0 To Me.actionDeathbed.Count - 1
                        tempList.Add(Me.actionDeathbed(x))
                    Next
                Case 3
                    For x = 0 To Me.actionFurthestRing.Count - 1
                        tempList.Add(Me.actionFurthestRing(x))
                    Next
            End Select

            Select Case areaToCopyInto
                Case -1
                    For x = 0 To tempList.Count - 1
                        Me.initialRegistry.Add(tempList(x))
                    Next
                Case 0
                    For x = 0 To tempList.Count - 1
                        Me.playerHand.Add(tempList(x))
                    Next
                Case 1
                    For x = 0 To tempList.Count - 1
                        Me.actionRegistry.Add(tempList(x))
                    Next
                Case 2
                    For x = 0 To tempList.Count - 1
                        Me.actionDeathbed.Add(tempList(x))
                    Next
                Case 3
                    For x = 0 To tempList.Count - 1
                        Me.actionFurthestRing.Add(tempList(x))
                    Next
            End Select
        End Sub

        Public Sub DrawActions(drawAmount As Integer, turnStart As Boolean)
            If turnStart = False And Me.cardDrawDouble > 0 And drawAmount > 0 Then
                For x = 1 To Me.cardDrawDouble
                    drawAmount += drawAmount
                Next
                Me.cardDrawDouble = 0
            End If


            For x = 1 To drawAmount
                If Me.actionRegistry.Count > 0 Then
                    Me.playerHand.Add(Me.actionRegistry(0))
                    Me.actionRegistry.RemoveAt(0)
                Else
                    If Me.actionDeathbed.Count <> 0 Then
                        MoveAreaIntoAnotherArea(2, 1)
                        ShuffleArea(1)
                        Me.playerHand.Add(Me.actionRegistry(0))
                        Me.actionRegistry.RemoveAt(0)
                    End If
                End If
            Next
        End Sub

        Public Sub WriteArea(areaToWrite)
            Select Case areaToWrite
                Case 0
                    For x = 0 To Me.playerHand.Count - 1
                        Console.WriteLine(Me.playerHand(x))
                    Next
                Case 1
                    For x = 0 To Me.actionRegistry.Count - 1
                        Console.WriteLine(Me.actionRegistry(x))
                    Next
                Case 2
                    For x = 0 To Me.actionDeathbed.Count - 1
                        Console.WriteLine(Me.actionDeathbed(x))
                    Next
                Case 3
                    For x = 0 To Me.actionFurthestRing.Count - 1
                        Console.WriteLine(Me.actionFurthestRing(x))
                    Next
            End Select
        End Sub 'ONLY USED FOR DEBUG

        Public Sub WriteAllAreas()
            Console.WriteLine("-=-=-=-")
            Console.WriteLine("Player Hand:")
            WriteArea(0)
            BlankLine()
            Console.WriteLine("Action Registry:")
            WriteArea(1)
            BlankLine()
            Console.WriteLine("Action Deathbed:")
            WriteArea(2)
            BlankLine()
            Console.WriteLine("Action Furthest Ring:")
            WriteArea(3)
            Console.WriteLine("-=-=-=-")
            BlankLine()
        End Sub 'ONLY USED FOR DEBUG

        Public Sub WriteAction(actionNo As Integer, actionToWrite As Integer)
            Dim actionName As String = ReadTextFile("\data\actionnames.txt", actionToWrite + 1)
            Dim actionCost As String = ReadTextFile("\data\actioncosts.txt", actionToWrite + 1)
            Dim actionDesc As String = ReadTextFile("\data\actiondescriptions.txt", actionToWrite + 1)

            Console.WriteLine(actionNo & ": [" & actionName & "] - (" & actionCost & ")")
            Console.WriteLine(actionDesc)
        End Sub

        Public Function WriteAreaChoice(area As Integer, additionalText As String, endTurn As Boolean) 'ADD: make this work with other areas, not only player hand
            Dim int As Integer
            Dim inputMax As Integer = 0
            Dim inputMin As Integer = 1
            Console.WriteLine("Choose action " & additionalText & ":")
            BlankLine()

            Select Case area
                Case 0
                    For x = 0 To playerHand.Count - 1
                        WriteAction(x + 1, playerHand(x))
                        BlankLine()
                    Next
                    inputMax = playerHand.Count
                Case 1
                    For x = 0 To actionRegistry.Count - 1
                        WriteAction(x + 1, actionRegistry(x))
                        BlankLine()
                    Next
                    inputMax = actionRegistry.Count
                Case 2
                    For x = 0 To actionDeathbed.Count - 1
                        WriteAction(x + 1, actionDeathbed(x))
                        BlankLine()
                    Next
                    inputMax = actionDeathbed.Count
                Case 3
                    For x = 0 To actionFurthestRing.Count - 1
                        WriteAction(x + 1, actionFurthestRing(x))
                        BlankLine()
                    Next
                    inputMax = actionFurthestRing.Count
            End Select

            If endTurn Then
                Console.WriteLine("0: End turn.")
                inputMin = -4
            End If

            Do
                BlankLine()
                Console.WriteLine("============")
                BlankLine()
                Console.Write("Input action: ")
                int = getCleanNumericalInput()
            Loop Until int >= inputMin And int <= inputMax


            BlankLine()
            Console.WriteLine("============")




            Return int
        End Function

        Public Sub TakeTurn()
            Dim allEnemiesDead As Boolean = True

            If Me.alive Then
                StartOfTurn()
                'Player takes over here
                Dim hasPassedTurn As Boolean = False
                Do
                    If hasInfiniteMaxQu = True Then
                        Me.maxQu = 200000000
                        Me.currentQu = 200000000
                    End If
                    If Me.currentHealth > Me.maxHealth Then
                        Me.currentHealth = Me.maxHealth
                    End If

                    Dim chosenAction As Integer '-1 = no input, 0 = passed turn, 1+ = action

                    RefreshCombatUI()
                    chosenAction = WriteAreaChoice(0, "to play", True)
                    RefreshCombatUI()

                    If chosenAction = 0 Then
                        hasPassedTurn = True
                    ElseIf chosenAction = -2 Then 'DEBUG ACTIONS
                        Console.Write("DEBUG ACTION: ")
                        ResolveAction(getCleanNumericalInput(), True, False)
                        Console.WriteLine("Enter to continue.")
                        Console.ReadLine()
                        'Console.Clear()
                    ElseIf chosenAction = -3 Then
                        Console.WriteLine("GIGADAMAGE CHEAT!!!")
                        For x = 0 To enemyList.Count - 1
                            DealDamageToEnemy(1111, x)
                        Next
                        Console.WriteLine("Enter to continue.")
                        Console.ReadLine()
                        'Console.Clear()
                    ElseIf chosenAction = -4 Then
                        Console.WriteLine("Oops...")
                        TakeDamage(1111)
                        Console.WriteLine("Enter to continue.")
                        Console.ReadLine()
                        'Console.Clear()
                    ElseIf chosenAction = -1 Then
                        'Console.Clear()
                    Else

                        If chosenAction > Me.playerHand.Count Then 'Checks if chosen action exists in hand
                            Console.WriteLine("Invalid choice")
                        Else
                            Dim chosenActionID As Integer = Me.playerHand(chosenAction - 1)
                            Dim chosenActionCost As Integer = ReadTextFile("\data\actionintegercosts.txt", chosenActionID + 1)

                            If currentQu >= chosenActionCost Or hasInfiniteMaxQu = True Then 'checks that Qu cost is met

                                currentQu -= chosenActionCost 'Qu Payment

                                Console.WriteLine(Me.firstName & " plays " & ReadTextFile("\data\actionnames.txt", chosenActionID + 1) & "!")

                                If chosenActionCost <> 0 Then
                                    Console.WriteLine(Me.firstName & " loses " & chosenActionCost & " Quintessence.")
                                End If

                                Me.playerHand.RemoveAt(chosenAction - 1)
                                ResolveAction(chosenActionID, True, False)
                            Else
                                Console.WriteLine("You don't have enough Quintessence to play " & ReadTextFile("\data\actionnames.txt", chosenActionID + 1) & ".")

                            End If
                        End If

                        Console.WriteLine("Enter to continue.")
                        Console.ReadLine()
                        'Console.Clear()
                    End If

                    allEnemiesDead = True
                    For x = 0 To enemyList.Count - 1
                        If enemyList(x).alive = True Then
                            allEnemiesDead = False
                        End If
                    Next

                Loop Until hasPassedTurn = True Or allEnemiesDead = True

                EndOfTurn()
            Else
                Console.WriteLine(Me.firstName & " is still dead.") 'should never be able to get to this code
                Console.ReadLine()
            End If
        End Sub

        Public Sub RefreshCombatUI()
            DrawCombatUI(False)
            For x = 1 To 55
                Console.WriteLine(Space(200))
            Next
            DrawCombatUI(True)
        End Sub

        Public Sub DrawCombatUI(withEnemy As Boolean)
            Console.SetCursorPosition(0, 0)
            WriteShortPlayerInfo()
            BlankLine()
            If withEnemy Then
                Console.WriteLine(Space(30) & "Alive Enemies:" & Space(30))
                DisplayEnemyInfo()
            End If
        End Sub

        Public Sub StartOfCombat()
            'Quintessence related initialisation
            Me.maxQu = Me.initialMaxQu
            Me.currentQu = Me.maxQu
            Me.QuStartTurnGain = 0
            Me.currentHandSize = Me.initialHandSize
            Me.extraTurnAfterCurrentTurn = 0

            'DEBUG STUFF
            'Me.hasInfiniteMaxQu = True 'ONLY SET TO TRUE FOR DEBUG PURPOSES
            'DebugRegistrySetup() 'ALSO ONLY FOR DEBUG PURPOSES
            'Me.maxQu = 100
            'Me.currentQu = 100

            Me.actionRegistry.Clear()
            CopyAreaIntoAnotherArea(-1, 1)
            ShuffleArea(1)
        End Sub

        Public Sub StartOfTurn()
            DrawActions(Me.currentHandSize, True)
            Me.currentQu = Me.maxQu + Me.QuStartTurnGain
            Me.QuStartTurnGain = 0
            If Me.currentQu < 0 Then
                Me.currentQu = 0
            End If
            If Me.maxQu < 0 Then
                Me.maxQu = 0
            End If

            Dim statusEffectChange As Integer = -1
            'ADD: Check for temporal status effects
            If temporalStatusEffects(0) > 1 Then
                statusEffectChange = 0
            ElseIf temporalStatusEffects(1) > 1 Then
                statusEffectChange = 1
            ElseIf temporalStatusEffects(2) > 1 Then
                statusEffectChange = 2
            End If

            For x = 1 To 512 'Lowering all status effects by determined amount
                If x <> 15 And Me.statusEffects(x) > 0 Then
                    Me.statusEffects(x) += statusEffectChange
                    If Me.statusEffects(x) < 0 Then
                        Me.statusEffects(x) = 0
                    End If
                End If
            Next

            For x = 1 To 512 'Lowering all temporal status effects by 1
                Me.temporalStatusEffects(x) -= 1
                If Me.temporalStatusEffects(x) < 0 Then
                    Me.temporalStatusEffects(x) = 0
                End If
            Next

            Me.accumulateList.Clear()
        End Sub

        Public Sub EndOfCombat()
            Me.maxHealth = Me.initialMaxHealth
            Me.hasInfiniteMaxQu = False
            Me.maxQu = Me.initialMaxQu
            Me.actionRegistry.Clear()
            Me.actionDeathbed.Clear()
            Me.actionFurthestRing.Clear()
            Me.accumulateList.Clear()
            Me.cardDrawDouble = 0
            Me.playerHand.Clear()

            For x = 1 To 512
                Me.statusEffects(x) = 0
                Me.temporalStatusEffects(x) = 0
            Next
        End Sub

        Public Sub EndOfTurn()
            'Console.Clear()

            Dim poisonText As Boolean = False
            If statusEffects(14) > 0 Then
                Console.WriteLine(Me.firstName & " is poisoned!")
                TakeDamage(statusEffects(14))
                poisonText = True
            End If
            If statusEffects(15) > 0 Then
                Console.WriteLine(Me.firstName & " is withering away!")
                TakeDamage(statusEffects(15))
                poisonText = True
            End If
            If poisonText Then
                Console.ReadKey()
            End If

            MoveAreaIntoAnotherArea(0, 2)

            If extraTurnAfterCurrentTurn > 0 Then
                extraTurnAfterCurrentTurn += -1
                StartOfTurn()
            End If
        End Sub

        Public Sub ResolveAction(ActionID As Integer, accumulateAction As Boolean, resolveViaEffectOfOtherAction As Boolean)

            'accumulateAction - True by default
            'resolveViaEffectOfOtherAction - False by default

            Dim noOfCopiesToDeathbed As Integer = 1
            Dim noOfActionResolves As Integer = 1 + Me.nextActionAdditionalResolve
            Me.nextActionAdditionalResolve = 0

            If resolveViaEffectOfOtherAction = True Then
                Console.WriteLine("The effect of " & ReadTextFile("\data\actionnames.txt", ActionID + 1) & " occured!")
            End If

            For resolveNo = 1 To noOfActionResolves
                Select Case ActionID
                    Case 0 'Abstain - Do nothing.
                        Console.WriteLine(Me.firstName & " does nothing...")
                        If 1 = Math.Ceiling(Rnd() * 1000) Then '1/1000 chance to play easter egg
                            Beat()
                            For x = 1 To Math.Ceiling(Rnd() * 5) + 1
                                Console.WriteLine("...")
                                Beat()
                            Next
                            Console.WriteLine("Okay, you can stop that now.")
                            Beat()
                        End If
                    Case 1 'Aggrieve - Deal 3 damage to target enemy.
                        DealDamageToEnemy(3, ChooseTargetEnemy(False))
                    Case 2 'Abjure - Gain Hardened 2.
                        GainStatusEffect(6, 2)
                        Console.WriteLine(Me.firstName & " hardened up!")
                    Case 3 'Abscond - Leave combat.
                    'ADD: End Combat early with no rewards
                    Case 4 'Aetherize - Increase your Max Quintessence by 1.
                        Me.maxQu += 1
                        Console.WriteLine(Me.firstName & "'s Max Quintessence increases by 1!")
                    Case 5 'Adduce - Choose a card in your hand and shuffle it and 4 extra copies of it into your deck.
                        Dim chosenAction As Integer
                        Do
                            chosenAction = WriteAreaChoice(0, "to shuffle it and 4 copies of it into your registry", False)
                            RefreshCombatUI()
                        Loop While chosenAction <= 0 Or chosenAction > Me.playerHand.count
                        Dim chosenActionID As Integer = Me.playerHand(chosenAction - 1)
                        Me.playerHand.RemoveAt(chosenAction - 1)
                        For x = 1 To 4
                            Me.actionRegistry.Add(chosenActionID)
                        Next
                        Console.WriteLine(Me.firstName & " shuffles " & ReadTextFile("\data\actionnames.txt", chosenActionID + 1) & " and four additional copies of it into their registry!")
                        ShuffleArea(1)
                    Case 6 'Alter - Decrease your Max Qu by 1, down to a minimum of 0. Gain 4 Qu.
                        Me.maxQu -= 1
                        Me.currentQu += 4
                        If maxQu < 0 Then
                            maxQu = 0
                        End If
                        Console.WriteLine(Me.firstName & "'s Max Quintessence decreases to " & Me.maxQu & ", and they gain 4 Quintessence!")
                    Case 7 'Ascend - Void a copy of this and draw an action. Then, if there are 5 copies of this in your furthest ring, gain infinite Max Quintessence, and refill your Quintessence.
                        Dim ascendCopiesInVoid As Integer = 0
                        AddActionToArea(7, 3)
                        DrawActions(1, False)
                        For x = 0 To Me.actionFurthestRing.Count - 1
                            If Me.actionFurthestRing(x) = 7 Then
                                ascendCopiesInVoid += 1
                            End If
                        Next
                        Console.WriteLine(Me.firstName & " voids an extra copy of Ascend and draws an action.")
                        Console.WriteLine("There are now " & ascendCopiesInVoid & " copies of Ascend in their furthest ring.")
                        If ascendCopiesInVoid > 4 Then
                            If Me.hasInfiniteMaxQu = False Then
                                Me.hasInfiniteMaxQu = True
                                Console.WriteLine(Me.firstName & " ascends beyond all understanding of energy, gaining infinite Quintessence!")
                            Else
                                Console.WriteLine(Me.firstName & " has already ascended, but I guess another infinity wouldn't hurt.")
                            End If
                        End If
                    Case 8 'Amplify - Repeat the next action you play.
                        Me.nextActionAdditionalResolve += 1
                        Console.WriteLine(Me.firstName & "'s next action will resolve twice.")
                    Case 9 'Ace - Draw 3 actions.
                        DrawActions(3, False)
                        Console.WriteLine(Me.firstName & " draws 3 actions.")
                    Case 10 'Acquaint - Move an action from your registry to your hand, then shuffle your registry.
                        Dim chosenAction As Integer = WriteAreaChoice(1, "to move into your hand", False)
                        Dim chosenActionID As Integer = Me.actionRegistry(chosenAction - 1)
                        Me.actionRegistry.RemoveAt(chosenAction - 1)
                        Me.playerHand.Add(chosenActionID)
                        ShuffleArea(1)
                        Console.WriteLine(Me.firstName & " moved " & ReadTextFile("\data\actionnames.txt", chosenActionID + 1) & " from their registry to their hand!")
                    Case 11 'Ameliorate - Heal yourself for 20% of your max health.
                        Me.currentHealth += Me.maxHealth / 5
                        Console.WriteLine(Me.firstName & " healed for " & Me.maxHealth / 5 & " health!")
                    Case 12 'Avail - Heal each enemy to full health; for every 10 health healed this way, increase your Max Qu by 1 and heal yourself for 5.
                        Dim healthHealed As Integer = 0
                        Dim QuGain As Integer = 0
                        Dim HealthGain As Integer = 0
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive = True Then
                                healthHealed += enemyList(x).maxHP - enemyList(x).currentHP
                                enemyList(x).currentHP = enemyList(x).maxHP
                            End If
                        Next
                        If healthHealed >= 10 Then
                            Do
                                QuGain += 1
                                HealthGain += 5
                                healthHealed += -10
                            Loop Until healthHealed < 10
                        End If
                        Me.maxQu += QuGain
                        Me.currentHealth += HealthGain
                        Console.WriteLine(Me.firstName & " healed all enemies back to full health,")
                        Console.WriteLine("increasing their max Quintessence by " & QuGain & " and healing themselves for " & HealthGain & "!")
                    Case 13 'Actus Reus - Gain 1 Qu for each alive enemy.
                        Dim QuGain As Integer = 0
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive = True Then
                                QuGain += 1
                            End If
                        Next
                        Me.currentQu += QuGain
                        Console.WriteLine(Me.firstName & " stole a total of " & QuGain & " Quintessence from their foes!")
                        Console.WriteLine("It's not like they were going to use it anyways.")
                        Console.WriteLine("Probably.")
                    Case 14 'Assess - Deal 2 damage to each enemy, then average each enemies current health. Enemies can't go above their maximum health this way.
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                DealDamageToEnemy(2, x)
                            End If
                        Next
                        Dim averageEnemyHealth As Integer = 0
                        Dim noOfEnemies As Integer = 0
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive = True Then
                                averageEnemyHealth += enemyList(x).currentHP
                                noOfEnemies += 1
                            End If
                        Next
                        averageEnemyHealth = averageEnemyHealth / noOfEnemies
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive = True Then
                                enemyList(x).currentHP = averageEnemyHealth
                            End If
                        Next
                        Console.WriteLine("The enemies have their health averaged, leaving them all with " & averageEnemyHealth & " health!")
                    Case 15 'Abandon - Increase your Max Quintessence by 3 and void your registry.
                        Me.maxQu += 3
                        MoveAreaIntoAnotherArea(1, 3)
                        Console.WriteLine(Me.firstName & " increased thier max Quintessence by 3, and voided thier Action Registry!")
                    Case 16 'Ridiculous Folly! - If your deathbed and registry are both empty, deal 1000 to target enemy, then shuffle all of your actions (whereever they are) into your registry. Else, void your hand, not including this.
                        If Me.actionRegistry.Count = 0 And Me.actionDeathbed.Count = 0 Then
                            Dim trgtenemy As Integer = ChooseTargetEnemy(False)
                            DealDamageToEnemy(1000, trgtenemy)
                            For x = 0 To 3
                                MoveAreaIntoAnotherArea(x, 1)
                            Next
                            ShuffleArea(1)
                            Console.WriteLine(Me.firstName & " engages in some simply RIDICULOUS FOLLY!")
                            Console.WriteLine("These shenanigans cause " & enemyList(trgtenemy).name & " to disappear from the battle...")
                            Console.WriteLine("Huh.")
                        Else
                            MoveAreaIntoAnotherArea(0, 3)
                            Console.WriteLine(Me.firstName & " voids their hand!")
                        End If
                    Case 17 'Antediluvianate - Add your deathbed to your hand.
                        MoveAreaIntoAnotherArea(2, 0)
                        Console.WriteLine(Me.firstName & " moves all of the actions in their deathbed into their hand.")
                    Case 18 'Accumulate - Choose an action in your hand. That action's effect occurs after each action played this turn.
                        Dim chosenAction As Integer = WriteAreaChoice(0, "to repeat the effect of after each action played this turn", False)
                        Dim chosenActionID As Integer = Me.playerHand(chosenAction - 1)
                        Me.accumulateList.Add(chosenActionID)
                        Console.WriteLine("Whenever " & Me.firstName & " plays an action for the rest of this turn, the effect of " & ReadTextFile("\data\actionnames.txt", chosenActionID + 1) & " will occur!")
                        accumulateAction = False
                    Case 19 'Arise - End your current turn, and immediately take an extra turn.
                        Dim x As String = Console.ForegroundColor
                        Me.EndOfTurn()
                        Me.StartOfTurn()
                        Console.WriteLine(Me.firstName & " brandishes a blade, and stabs themself through the heart!")
                        Beat()
                        Console.Write(".")
                        Console.ForegroundColor = ConsoleColor.Gray
                        Beat()
                        Console.Write(".")
                        Console.ForegroundColor = ConsoleColor.DarkGray
                        Beat()
                        Console.WriteLine(".")
                        Beat()
                        Console.ForegroundColor = ConsoleColor.White
                        Console.WriteLine("They feel a pull to another place; another being.")
                        Beat()
                        Console.WriteLine("A familiar being.")
                        Beat()
                        SetTextToEyeColour(Me.eyeColour)
                        Console.WriteLine("Themselves.")
                        Beat()
                        'Console.Clear()
                        Console.ForegroundColor = x
                    Case 20 'Apprehend - Next time you draw an amount of cards besides at the start of your turn, draw twice as many.
                        Me.cardDrawDouble += 1
                        Console.WriteLine("Next time " & Me.firstName & " draws any amount of cards, they will draw twice as many.")
                    Case 21 'Auspicate - Play the top card of your registry for free.
                        If Me.actionRegistry.Count <> 0 Then
                            Console.WriteLine("The top card of " & Me.firstName & "'s action registry was " & ReadTextFile("\data\actionnames.txt", Me.actionRegistry(0) + 1) & "!")
                            ResolveAction(Me.actionRegistry(0), False, True)
                            Me.actionRegistry.RemoveAt(0)
                        Else
                            Console.WriteLine(Me.firstName & "'s action registry is empty... no action to play.")
                        End If
                    Case 22 'Aggravate - Gain Puissance 1.
                        GainStatusEffect(2, 1)
                        Console.WriteLine(Me.firstName & " gains 1 Puissance!")
                    Case 23 'Asylumnate - Feel lucky and draw 1.
                        Me.nextLuckAdvantage += 1
                        Me.stats(11) += 1
                        DrawActions(1, False)
                        Console.WriteLine(Me.firstName & " draws an action!")
                    Case 24 'Aught - Swap your hand with the actions in the furthest ring, then draw a card for every Abstain you moved to your furthest ring with this.
                        Dim tempList As New List(Of Integer)
                        Dim amountOfAbstains As Integer = 0

                        For x = 0 To Me.playerHand.Count - 1
                            tempList.Add(Me.playerHand(x))
                        Next
                        Me.playerHand.Clear()

                        MoveAreaIntoAnotherArea(3, 0)

                        For x = 0 To tempList.Count - 1
                            amountOfAbstains += 1
                            Me.actionFurthestRing.Add(tempList(x))
                        Next

                        DrawActions(amountOfAbstains, False)

                        Console.WriteLine(Me.firstName & " swaps thier hand with the actions in their furthest ring, and then draws " & amountOfAbstains & " actions!")
                    Case 25 'Aureole - Increase your actions drawn each turn by 1.
                        Me.currentHandSize += 1
                        Console.WriteLine(Me.firstName & "'s hand size increases by 1!")
                    Case 26 'Accord - Gain Invulnerable 2 and Unable 2.
                        Me.GainStatusEffect(5, 2)
                        Me.GainStatusEffect(8, 2)
                        Console.WriteLine(Me.firstName & " cannot be hurt, nor hurt anyone else, for 2 turns!")
                    Case 27 'Alleviate - Double your max HP.
                        Me.maxHealth += Me.maxHealth
                        Me.currentHealth += Me.maxHealth / 20
                        Console.WriteLine(Me.firstName & "'s max health doubled!")
                        Console.WriteLine(Me.firstName & " healed for " & Me.maxHealth / 20 & " health!")
                    Case 28 'Amor Fati - Increase your Max Quintessence by 2, then give yourself Wither 3.
                        Me.maxQu += 2
                        GainStatusEffect(15, 3)
                        Console.WriteLine(Me.firstName & " increased thier max Quintessence by 2, but gained 3 Wither!")
                    Case 29 'Psychnnihilate - Become invincible for the rest of the turn, then take 1000 damage.
                        GainStatusEffect(8, 1)
                        Console.WriteLine(Me.firstName & " prepares to take a ridiculous amount of damage...")
                        TakeDamage(1000)
                        Console.WriteLine("Psyche!")
                    Case 30 'Self Destruct - Take 1000 damage.
                        Console.WriteLine(Me.firstName & " holds their breath and thinks about imploding really hard...")
                        TakeDamage(1000)
                        If Me.alive = False Then
                            Console.WriteLine("It worked.")
                        End If
                    Case 31 'Salubritighten - Lower all enemies Max HP to their current HP.
                        For x = 0 To enemyList.Count - 1
                            Dim hadEffect As Boolean = False
                            If enemyList(x).maxHP <> enemyList(x).currentHP Then
                                Console.WriteLine(enemyList(x).name & "'s Max HP was lowered from " & enemyList(x).maxHP & " to " & enemyList(x).currentHP & "!")
                                enemyList(x).maxHP = enemyList(x).currentHP
                                hadEffect = True
                            End If
                            If hadEffect = False Then
                                Console.WriteLine("It did nothing.")
                            End If
                        Next
                    Case 32 'Feathercadence - Unused

                    Case 33 'Ivories in the Fire - Unused

                    Case 34 'Mixolydian Maelstrom - Unused

                    Case 35 'Fantasia's Inhale - Unused

                    Case 36 'Adagio Redshift - Unused

                    Case 37 'Disasterpiece - Swap all of your stats around. (Permanently!)
                        Dim tempStats As New List(Of Integer)
                        Dim i As Integer
                        For x = 1 To NoOfStats
                            tempStats.Add(stats(x))
                        Next
                        tempStats(Math.Ceiling(Rnd() * tempStats.Count)) += 1
                        For x = 1 To NoOfStats
                            i = Math.Ceiling(Rnd() * tempStats.Count) - 1
                            stats(x) = tempStats(i)
                            tempStats.RemoveAt(i)
                        Next
                        Console.WriteLine(Me.firstName & "'s stats all swapped around!")
                    Case 38 'Clobber - Deal 23 damage.
                        DealDamageToEnemy(23, ChooseTargetEnemy(False))
                            Case 39 'Smack - Deal 7 damage and draw 2.
                        DealDamageToEnemy(7, ChooseTargetEnemy(False))
                            DrawActions(2, False)
                        Console.WriteLine(Me.firstName & " draws 2 actions.")
                    Case 40 'Bash - Deal 5.0X damage.
                        DealDamageToEnemy(5 * ChooseXMana(), ChooseTargetEnemy(False))
                            Case 41 'Thwack - Deal 7 damage.
                        DealDamageToEnemy(7, ChooseTargetEnemy(False))
                            Case 42 'Wallop - Deal 50 damage!
                        DealDamageToEnemy(50, ChooseTargetEnemy(False))
                    Case 43 'Bonk - Deal 5 damage and inflict Confuse 1.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(False)
                        DealDamageToEnemy(5, targetEnemy)
                        enemyList(targetEnemy).statusEffects(12) += 1
                    Case 44 'Slug - Inflict Nonhardy 1, then deal 4 damage.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(False)
                        enemyList(targetEnemy).statusEffects(9) += 1
                        DealDamageToEnemy(4, targetEnemy)
                    Case 45 'Thump - Deal 5 damage, then gain Sinew 1.
                        DealDamageToEnemy(5, ChooseTargetEnemy(False))
                        GainStatusEffect(1, 1)
                    Case 46 'Sock - Deal 5 - 10 damage.
                        DealDamageToEnemy(Math.Ceiling(Rnd() * 6) + 4, ChooseTargetEnemy(False))
                    Case 47 'Discombobulate - Inflict Confuse 3.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(False)
                        enemyList(targetEnemy).statusEffects(12) += 3
                    Case 48 'Disorientuple - Deal 10 damage to all enemies and inflict Confuse 1 onto all enemies.
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                DealDamageToEnemy(10, x)
                                enemyList(x).statusEffects(12) += 1
                            End If
                        Next
                    Case 49 'Ventilate - Discard your hand, then draw equal to the number of cards discarded.
                        Dim handCount As Integer = playerHand.Count
                        MoveAreaIntoAnotherArea(0, 2)
                        DrawActions(handCount, False)
                        Console.WriteLine(Me.firstName & " discards their hand and draws " & handCount & " actions.")
                    Case 50 'Expound - Draw X, then deal damage to target enemy equal to total Qu cost of all drawn actions.
                        Dim XQu As Integer = ChooseXMana()
                        Dim drawnActionsCost As Integer = 0
                        For x = 0 To XQu - 1
                            drawnActionsCost += ReadTextFile("\data\actionintegercosts.txt", Me.actionRegistry(x) + 1)
                        Next
                        DrawActions(XQu, False)
                        Console.WriteLine(Me.firstName & " draws " & XQu & " actions, with a total Quintessence cost of " & drawnActionsCost & "!")
                        DealDamageToEnemy(drawnActionsCost, ChooseTargetEnemy(False))
                    Case 51 'Enunciate - Draw 1, then Draw equal to the Qu cost of the drawn card.
                        Dim drawnActionCost As Integer = ReadTextFile("\data\actionintegercosts.txt", Me.actionRegistry(0) + 1)
                        DrawActions(1, False)
                        Console.WriteLine(Me.firstName & " draws an action...")
                        Console.WriteLine("The action drawn costs " & drawnActionCost & " Quintessence.")
                        Console.WriteLine(Me.firstName & " draws an additional " & drawnActionCost & " action(s)!")
                        DrawActions(drawnActionCost, False)
                    Case 52 'Falter - Play a random action from your hand without paying its Qu cost.
                        If Me.playerHand.Count <> 0 Then
                            Dim chosenAction As Integer = WriteAreaChoice(0, "to play at no cost", False)
                            ResolveAction(Me.playerHand(chosenAction - 1), False, True)
                            Me.playerHand.RemoveAt(chosenAction - 1)
                        Else
                            Console.WriteLine(Me.firstName & "'s hand is empty... no actions to choose from.")
                        End If
                    Case 53 'Acuate - Reveal the top 4 cards from your registry. If any of them share Qu costs, play them both for free. Else, add them to your hand.
                        Console.WriteLine("The top 4 cards from your registry are...")
                        BlankLine()
                        DrawActions(4, False)
                        Dim counter As Integer = 1
                        Dim actionCosts(4) As Integer
                        Dim actionPlay(4) As Boolean
                        For x = Me.playerHand.Count - 4 To Me.playerHand.Count - 1
                            WriteAction(counter, Me.playerHand(x))
                            actionCosts(counter) = ReadTextFile("\data\actionintegercosts.txt", Me.playerHand(x) + 1)
                            actionPlay(counter) = False
                            counter += 1
                        Next
                        BlankLine()
                        For x = 1 To 4
                            For y = 1 To 4
                                If x <> y And actionCosts(x) = actionCosts(y) Then
                                    actionPlay(x) = True
                                    actionPlay(y) = True
                                End If
                            Next
                        Next
                        counter = 1
                        Dim actionsPlayed As Integer = 0
                        For x = Me.playerHand.Count - 4 To Me.playerHand.Count - 1
                            If actionPlay(counter) Then
                                ResolveAction(Me.playerHand(x - actionsPlayed), False, True)
                                Me.playerHand.RemoveAt(x - actionsPlayed)
                                actionsPlayed += 1
                            End If
                            counter += 1
                        Next
                    Case 54 'Corrupt - Inflict 4 poison upon target enemy.
                        Me.stats(10) -= 1
                        Dim targetEnemy As Integer = ChooseTargetEnemy(False)
                        enemyList(targetEnemy).statusEffects(14) += 4
                    Case 55 'Tripunch - Deal 4 damage 3 times, choosing targets each time.
                        For i = 0 To 2
                            DealDamageToEnemy(4, ChooseTargetEnemy(False))
                        Next
                    Case 56 'Obduration - Halt the decreasing of all status effects after each turn for 3 turns.
                        Me.temporalStatusEffects(0) += 3
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                enemyList(x).temporalStatusEffects(0) += 3
                            End If
                        Next
                    Case 57 'Deja Vu - Place your deathbed ontop of your registry.
                        MoveAreaIntoAnotherArea(2, 1)
                        Console.WriteLine(Me.firstName & "'s deathbed was placed ontop of thier registry.")
                    Case 58 'Replenish - Draw back up to your hand size.
                        If Me.playerHand.Count >= Me.currentHandSize Then
                            Console.WriteLine("The number of actions currently in your hand is greater than or equal to your hand size, so no actions are drawn.")
                        Else
                            Console.WriteLine(Me.firstName & " draws " & Me.currentHandSize - Me.playerHand.Count & " action(s)!")
                            DrawActions(Me.currentHandSize - Me.playerHand.Count, False)
                        End If
                    Case 59 'Equivalessence - Draw until you have drawn cards of total Qu cost 6 or more.
                        Dim totalQuCosts As Integer = 0
                        Do
                            DrawActions(1, False)
                            Console.WriteLine(Me.firstName & " draws an action with a Quintessence cost of " & ReadTextFile("\data\actionintegercosts.txt", Me.playerHand(Me.playerHand.Count - 1) + 1) & ".")
                            totalQuCosts += ReadTextFile("\data\actionintegercosts.txt", Me.playerHand(Me.playerHand.Count - 1) + 1)
                        Loop Until totalQuCosts >= 6
                        Console.WriteLine("Totalled cost of 6 or more Quintessence reached!")
                    Case 60 'Aristobtain - Draw 5 actions, void any of them with Qu costs of 1 or less.
                        Dim exiledCards As Integer = 0
                        Console.WriteLine(Me.firstName & " draws 5 actions!")
                        DrawActions(5, False)
                        For x = Me.playerHand.Count - 5 To Me.playerHand.Count - 1
                            If ReadTextFile("\data\actionintegercosts.txt", Me.playerHand(x) + 1) <= 1 Then
                                Console.WriteLine(ReadTextFile("\data\actionnames.txt", Me.playerHand(x) + 1) & " has a Quintessence cost of 2 or less, so it is exiled.")
                                AddActionToArea(Me.playerHand(x), 3)
                                Me.playerHand(x) = -1
                                exiledCards += 1
                            End If
                        Next
                        For x = 1 To exiledCards
                            Me.playerHand.Remove(-1)
                        Next
                    Case 61 'Ourobaugment - Shuffle a copy of your registry into itself.
                        CopyAreaIntoAnotherArea(1, 1)
                        ShuffleArea(1)
                        Console.WriteLine(Me.firstName & "'s registry doubles in size, and shuffles itself!")
                    Case 62 'Flailment - Deal 1 damage to a random enemy, 25 times.
                        For i = 1 To 25
                            DealDamageToEnemy(1, ChooseTargetEnemy(True))
                        Next
                    Case 63 'Strangulate - Half target enemy's Max HP, then fully heal them.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).maxHP = enemyList(targetEnemy).maxHP / 2
                        enemyList(targetEnemy).currentHP = enemyList(targetEnemy).maxHP
                        Console.WriteLine(enemyList(targetEnemy).name & " has their max health slashed in half, and are then fully healed!")
                    Case 64 'Delimituple - Deal 10 damage to all enemies and lower their Max HP to thier current HP.
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                DealDamageToEnemy(10, x)
                                enemyList(x).maxHP = enemyList(x).currentHP
                            End If
                        Next
                    Case 65 'Chronocuff - Deal 18 damage to an enemy and prevent any status effects they have from decreasing for 3 turns.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        DealDamageToEnemy(18, targetEnemy)
                        enemyList(targetEnemy).temporalStatusEffects(0) += 3
                    Case 66 'Miasmaxe - Discard your hand, then inflict Poison equal to double the amount of cards you discarded.
                        Dim discardSize As Integer = Me.playerHand.Count
                        MoveAreaIntoAnotherArea(0, 2)
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).statusEffects(14) += discardSize * 2
                        Console.WriteLine(Me.firstName & " discards their hand of " & discardSize & " actions, and inflicts " & discardSize * 2 & " poison onto " & enemyList(targetEnemy).name & "!")
                    Case 67 'Gift of Oblivion - Spare an enemy from existance.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).maxHP = 0
                        Console.WriteLine(enemyList(targetEnemy).name & " ceases to exist within the same three spacial and single temporal dimensions that you do!")
                    Case 68 'Towards Nothingness - Emerge victorious.
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                enemyList(x).maxHP = 0
                            End If
                        Next
                        Console.WriteLine("All of your foes fold away into nothing, leaving you alone, victorious.")
                    Case 69 'Sap - Inflict Wither 3.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).statusEffects(15) += 3
                        Console.WriteLine(Me.firstName & " inflicts Wither 3 onto " & enemyList(targetEnemy).name & "!")
                    Case 70 'Spoil - Inflict Wither 1 to all enemies, and Poison 8 to target enemy
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).statusEffects(14) += 8
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                enemyList(x).statusEffects(15) += 1
                            End If
                        Next
                        Console.WriteLine(Me.firstName & " inflicts Poison 8 onto " & enemyList(targetEnemy).name & ", and Wither 1 to all enemies!")
                    Case 71 'Wilt - Inflict Poison 3.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).statusEffects(14) += 3
                        Console.WriteLine(Me.firstName & " inflicts Poison 3 onto " & enemyList(targetEnemy).name & "!")
                    Case 72 'Rot - Turn all Poison on enemies into Wither.
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                enemyList(x).statusEffects(15) += enemyList(x).statusEffects(14)
                                enemyList(x).statusEffects(14) = 0
                            End If
                        Next
                        Console.WriteLine("All Poison on enemies becomes Wither!")
                    Case 73 'Fester - Double all Poison & Wither effects on target enemy.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).statusEffects(14) += enemyList(targetEnemy).statusEffects(14)
                        enemyList(targetEnemy).statusEffects(15) += enemyList(targetEnemy).statusEffects(15)
                        Console.WriteLine("All Poison and Wither on " & enemyList(targetEnemy).name & " doubles!")
                    Case 74 'Indoughlge - Double target enemy's grist drop.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).gristDrop += enemyList(targetEnemy).gristDrop
                        Console.WriteLine(enemyList(targetEnemy).name & "'s grist drop doubles!")
                    Case 75 'Decomputrificatinate - Inflict 2X Wither to each enemy, and 2X Poison to yourself.
                        Dim chosenX As Integer = ChooseXMana()
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                enemyList(x).statusEffects(15) += chosenX * 2
                            End If
                        Next
                        GainStatusEffect(14, chosenX * 2)
                        Console.WriteLine(Me.firstName & " gained " & chosenX * 2 & " Poison, and each enemy gained " & chosenX * 2 & " Wither!")
                    Case 76 'Purgerocede - Transfer all negative status effects from yourself to target enemy.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        For x = 0 To 16
                            If x <> 0 And x <> 1 And x <> 2 And x <> 6 And x <> 7 And x <> 8 Then
                                enemyList(targetEnemy).statusEffects(x) += Me.statusEffects(x)
                            End If
                        Next
                        Console.WriteLine("All negative status effects on " & Me.firstName & " transfer over to " & enemyList(targetEnemy).name & "!")
                    Case 77 'Purify - Remove all status effects from yourself.
                        For x = 0 To 16
                            Me.statusEffects(x) = 0
                        Next
                        Console.WriteLine(Me.firstName & " loses all status effects!")
                    Case 78 'Venihilate - Deal 9 damage and inflict 3 Poison and Wither.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        DealDamageToEnemy(9, targetEnemy)
                        enemyList(targetEnemy).statusEffects(14) += 3
                        enemyList(targetEnemy).statusEffects(15) += 3
                    Case 79 'Inexorate - Inflict Wither to each enemy equal to a quarter of their current health.
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                enemyList(x).statusEffects(15) += enemyList(x).currentHP / 4
                            End If
                        Next
                        Console.WriteLine("Each enemy gets inflicted with Wither equal to a quarter of their health!")
                    Case 80 'Extirpate - Leave an enemy at 1 health.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).currentHP = 1
                        Console.WriteLine(enemyList(targetEnemy).name & "'s health is reduced to 1!")
                    Case 81 'Zip - Draw a card and increase your Max Quintessence by 1.
                        DrawActions(1, False)
                        Me.maxQu += 1
                        Console.WriteLine(Me.firstName & " draws an action, and increases their Max Quintessence by 1!")
                    Case 82 'Verve - Increase your Max Quintessence by X.
                        Dim chosenX As Integer = ChooseXMana()
                        Me.maxQu += chosenX
                        Console.WriteLine(Me.firstName & " increases their Max Quintessence by " & chosenX & "!")
                    Case 83 'Brio - Increase your Max Quintessence by 1 for each enemy with Poison or Wither.
                        Dim enemiesWithPoisonOrWither As Integer = 0
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                If enemyList(x).statusEffects(14) > 0 Or enemyList(x).statusEffects(15) > 0 Then
                                    enemiesWithPoisonOrWither += 1
                                End If
                            End If
                        Next
                        Me.maxQu += enemiesWithPoisonOrWither
                        Console.WriteLine(Me.firstName & " increases their Max Quintessence by " & enemiesWithPoisonOrWither & "!")
                    Case 84 'Zeal - Increase your Max Quintessence by 2 for each enemy with more health than you.
                        Dim enemiesWithMoreHealth As Integer = 0
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive And enemyList(x).currentHP > Me.currentHealth Then
                                enemiesWithMoreHealth += 2
                            End If
                        Next
                        Me.maxQu += enemiesWithMoreHealth
                        Console.WriteLine(Me.firstName & " increases their Max Quintessence by " & enemiesWithMoreHealth & "!")
                    Case 85 'Zest - Increase your Max Quintessence by 1, and increase your actions drawn each turn by 1.
                        Me.maxQu += 1
                        Me.currentHandSize += 1
                        Console.WriteLine(Me.firstName & " increases their Max Quintessence and hand size each by 1!")
                    Case 86 'Festvour - Gain 3 Qu and gain 3 Wither.
                        Me.currentQu += 3
                        GainStatusEffect(15, 3)
                        Console.WriteLine(Me.firstName & " gains 3 Quintessence and 3 Wither!")
                    Case 87 'Ardour - Gain 2 Qu and take 5 damage.
                        Me.currentQu += 2
                        TakeDamage(5)
                        Console.WriteLine(Me.firstName & " gains 2 Quintessence!")
                    Case 88 'Avidilation - Gain 2 Qu, generate 2 less Qu next turn.
                        Me.currentQu += 2
                        Me.QuStartTurnGain += -2
                        Console.WriteLine(Me.firstName & " steals 2 Quintessence from thier future self!")
                    Case 89 'Rapacify - Inflict Unable 3 to an enemy and gain a copy of thier grist drop.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).statusEffects(5) += 3
                        gristCache += enemyList(targetEnemy).gristDrop
                        Console.WriteLine(Me.firstName & " gains " & enemyList(targetEnemy).gristDrop & " Grist!")
                    Case 90 'Avarecede - Kill an enemy, but gain no grist from them.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).gristDrop = 0
                        enemyList(targetEnemy).maxHP = 0
                        Console.WriteLine(Me.firstName & " sends " & enemyList(targetEnemy).name & " hurtling into next week!")
                        Console.WriteLine("Unfortunatly they had a very good grip on thier grist stash.")
                    Case 91 'Gusto - Draw X, and generate X additional Qu next turn.
                        Dim chosenX As Integer = ChooseXMana()
                        DrawActions(chosenX, False)
                        Me.QuStartTurnGain += chosenX
                        Console.WriteLine(Me.firstName & " draws " & chosenX & " actions and sends " & chosenX & " Quintessence to thier future self!")
                    Case 92 'Parsimo-knee! - Deal 18 damage and gain 20 Grist.
                        DealDamageToEnemy(18, ChooseTargetEnemy(True))
                        gristCache += 20
                        Console.WriteLine(Me.firstName & " gained 20 Grist!")
                    Case 93 'Resistallay - Inflict Vulnerable 3.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).statusEffects(11) += 3
                        Console.WriteLine(Me.firstName & " inflicts Vulnerable 3 onto " & enemyList(targetEnemy).name & "!")
                    Case 94 'Widdershingles - Inflict Poison 2, and make all status effects increase instead of decrease at the end of turns for 2 turns.
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).statusEffects(14) += 2
                        Me.temporalStatusEffects(1) += 2
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                enemyList(x).temporalStatusEffects(1) += 2
                            End If
                        Next
                        Console.WriteLine(Me.firstName & " inflicts Poison 2 onto " & enemyList(targetEnemy).name & ", and all status effects increase instead of decrease for 2 turns.")
                    Case 95 'Mausoleviate - Shuffle a copy of your hand into your deathbed.
                        CopyAreaIntoAnotherArea(0, 2)
                        ShuffleArea(2)
                        Console.WriteLine("A copy of your hand is shuffled into your deathbed!")
                    Case 96 'Sepulchrevive - Shuffle your deathbed into your registry.
                        MoveAreaIntoAnotherArea(2, 1)
                        ShuffleArea(1)
                        Console.WriteLine("Your deathbed is shuffled into your registry!")
                    Case 97 '~Battle of Wits~ - If you have 200 or more actions in your registry, kill all enemies.
                        If Me.actionRegistry.Count >= 200 Then
                            For x = 0 To enemyList.Count - 1
                                If enemyList(x).alive Then
                                    enemyList(x).maxHP = 0
                                End If
                            Next
                            Console.WriteLine("Your opponents cower at the sheer mass of your intellect!")
                            Console.WriteLine("...")
                            Console.WriteLine("Actually it's the number of cards in your registry that scared them off, but you like to think otherwise.")
                        Else
                            Console.WriteLine("Your registry is not powerful enough yet...")
                        End If
                    Case 98 '~Time Walk~ - Take an extra turn after this one.
                        Me.extraTurnAfterCurrentTurn += 1
                        Console.WriteLine("Your future expands before you...")
                        Console.WriteLine("You have an extra turn after this one ends.")
                    Case 99 '~Black Lotus~ - Gain 3 Qu.
                        Me.currentQu += 3
                        Console.WriteLine("The flower opens up, dispersing an unknown energy into the surrounding space.")
                        Console.WriteLine("You harness the energy, gaining 3 Quintessence!")
                    Case 100 '~Ancestral Recall~ - Draw 3 actions.
                        DrawActions(3, False)
                        Console.WriteLine("You call to your ancestors for help...")
                        Console.WriteLine("They bless you, and you draw 3 actions!")
                    Case 101 '~One With Nothing~ - Discard your hand.
                        MoveAreaIntoAnotherArea(0, 2)
                        Console.WriteLine("You discard your hand.")
                        Console.WriteLine("When nothing remains, everything is equally possible.")
                    Case 102 '「Magic Cylinder」 - All damage taken from enemies until next turn is reflected evenly between them.
                        'Unused
                    Case 103 '「Solemn Judgement」 - Lose half of your HP and kill an enemy.
                        Me.currentHealth = Me.currentHealth / 2
                        Dim targetEnemy As Integer = ChooseTargetEnemy(True)
                        enemyList(targetEnemy).maxHP = 0
                        Console.WriteLine(enemyList(targetEnemy).name & " has been judged!")
                        Console.WriteLine("GUILTY!")
                    Case 104 '「One for One」 - Discard an action, and play an action from your registry.
                        'Unused
                    Case 105 '「Maxx "C"」 - Draw 1 extra action on your next turn for each alive enemy.
                        Dim noOfAliveEnemies As Integer = 0
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                noOfAliveEnemies += 1
                            End If
                        Next
                        'Unused - code unfinished
                    Case 106 '「Effect Veiler」 - Force an enemy to idle on their next turn.
                        'Unused - Idle status not coded
                    Case 107 '「Ash Blossom & Joyous Spring」 - Force all enemies to idle on their next turn.
                        'Unused - Idle status not coded
                    Case 108 '「Pot of Greed」 - Draw 2 actions.
                        DrawActions(2, False)
                        Console.WriteLine("You reach into the pot...")
                        Console.WriteLine("...and draw 2 actions!")
                    Case 109 '<Attune> - Gain 1 Qu.
                        Me.currentQu += 1
                        Console.WriteLine("You reach into the astral plane, where everything that ever was, or could be, exists.")
                        Console.WriteLine("You gain 1 Quintessence!")
                    Case 110 '<Iconoclasm> - Inflict Unable 2 to yourself and all enemies.
                        For x = 0 To enemyList.Count - 1
                            If enemyList(x).alive Then
                                enemyList(x).statusEffects(5) += 2
                            End If
                        Next
                        GainStatusEffect(5, 2)
                        Console.WriteLine("Every nearby being stops making any sound.")
                        Console.WriteLine("You're all rendered mute and incapable.")
                    Case 111 '<Harvest Fear> - Deal 6 damage. If this kills, increase all stats by 2.
                        'unused
                    Case 112 '<Harmony> - If your hand is comprised of 10 unique actions, kill all enemies.
                        'unused
                    Case 113 '<Fusion Project> - Play and then discard the top two cards from your registry.
                        'unused
                    Case 114 '<Ongoing Research> - Draw an action, then add a copy of this to your hand.
                        DrawActions(1, False)
                        AddActionToArea(114, 0)
                        Console.WriteLine("Science takes time...")
                        Console.WriteLine("You draw an action, and add a copy of <Ongoing Research> to your hand!")
                    Case 115 '<Put Down> - If it is your first turn, kill an enemy.
                        'unused
                    Case 116 'Mend - Heal for 25% of your max health.
                        Me.currentHealth += Me.maxHealth / 4
                        Console.WriteLine(Me.firstName & " healed for " & Me.maxHealth / 4 & " health!")
                    Case 117 'Revivify - Fully heal yourself.
                        Me.currentHealth += Me.maxHealth
                        Console.WriteLine(Me.firstName & " fully healed!")
                    Case 118 'Inspirit - Heal for 10X% of your max health.
                        Dim chosenX As Integer = ChooseXMana()
                        Me.currentHealth += (Me.maxHealth / 10) * chosenX
                        Console.WriteLine(Me.firstName & " healed for " & (Me.maxHealth / 10) * chosenX & " health!")
                    Case 119 'Resuscitate - Increase your Max Health by 10% and heal 15%.
                        Dim oldMax As Integer = Me.maxHealth
                        Me.maxHealth += Me.maxHealth / 10
                        Me.currentHealth += (Me.maxHealth / 20) * 3
                        Console.WriteLine(Me.firstName & " increased thier max health by " & oldMax / 10 & ", and healed for " & (Me.maxHealth / 20) * 3 & " health!")
                    Case 120 'Bandage - Heal for 5% of your max health.
                        Me.currentHealth += Me.maxHealth / 20
                        Console.WriteLine(Me.firstName & " healed for " & Me.maxHealth / 20 & " health!")
                    Case Else
                        Console.WriteLine("Something's gone wrong...")
                        BlankLine()
                        ErrorCustomMsg("Invalid Action ID given to resolve")
                End Select

                If accumulateAction Then
                    For x = 0 To Me.accumulateList.Count - 1
                        ResolveAction(Me.accumulateList(x), False, True)
                    Next
                Else
                    noOfCopiesToDeathbed = 0
                End If

                If noOfCopiesToDeathbed > 0 Then
                    For x = 1 To noOfCopiesToDeathbed
                        Me.actionDeathbed.Add(ActionID)
                    Next
                End If
            Next
        End Sub

        Public Sub CheckAlive()
            If Me.currentHealth <= 0 Then
                If Me.alive = True Then
                    Me.alive = False
                    Die()
                End If
            Else
                Me.alive = True
            End If
        End Sub

        Public Sub Die()
            Me.currentHealth = 0
            Console.WriteLine(Me.firstName & " has died!")
        End Sub

        Public Sub AddActionToArea(actionToAdd As Integer, area As Integer)
            Select Case area
                Case -1
                    Me.initialRegistry.Add(actionToAdd)
                Case 0
                    Me.playerHand.Add(actionToAdd)
                Case 1
                    Me.actionRegistry.Add(actionToAdd)
                Case 2
                    Me.actionDeathbed.Add(actionToAdd)
                Case 3
                    Me.actionFurthestRing.Add(actionToAdd)
            End Select
        End Sub

        Public Sub DealDamageToEnemy(damageAmount As Integer, toEnemy As Integer)

            If Not AnyEnemiesAlive() Then
                Exit Sub
            End If

            If Me.statusEffects(0) > 0 Then
                damageAmount = damageAmount * 1.2
            End If
            If Me.statusEffects(1) > 0 Then
                damageAmount = damageAmount * 1.5
            End If
            If Me.statusEffects(2) > 0 Then
                damageAmount = damageAmount * 2
            End If
            If Me.statusEffects(3) > 0 Then
                damageAmount = damageAmount * 0.8
            End If
            If Me.statusEffects(4) > 0 Then
                damageAmount = damageAmount * 0.5
            End If
            If Me.statusEffects(5) > 0 Then
                damageAmount = damageAmount * 0
            End If

            If toEnemy = -1 Then
                'ADD: Deal damage to random player
                Dim randomPlayer As Integer
                Dim validRandomPlayer As Boolean
                Do
                    validRandomPlayer = True
                    randomPlayer = Math.Ceiling(Rnd() * 4)

                    If playerarray(randomPlayer).alive = False Then
                        validRandomPlayer = False
                    End If

                Loop Until validRandomPlayer = True

                playerarray(randomPlayer).TakeDamage(damageAmount)
            End If

            Dim targetValid As Boolean = True
            If toEnemy > enemyList.Count - 1 Then 'Check target exists
                targetValid = False
            End If

            If enemyList(toEnemy).alive = False Then 'Check target is alive
                targetValid = False
            End If

            If targetValid Then 'if target is valid, deal damage, and enemy checks if they're still alive
                enemyList(toEnemy).TakeDamage(damageAmount)
                enemyList(toEnemy).CheckIfAlive()

                If enemyList(toEnemy).alive = False Then
                    Console.WriteLine(Me.firstName & " deals " & damageAmount & " damage to " & enemyList(toEnemy).name & ", killing them!")
                Else
                    Console.WriteLine(Me.firstName & " deals " & damageAmount & " damage to " & enemyList(toEnemy).name & "!")
                End If
            End If
        End Sub

        Public Sub TakeDamage(damageAmount As Double)
            Dim healthBeforeHit As Integer = Me.currentHealth
            If Me.statusEffects(6) > 0 Then
                damageAmount = damageAmount * 0.8
            End If
            If Me.statusEffects(7) > 0 Then
                damageAmount = damageAmount * 0.5
            End If
            If Me.statusEffects(8) > 0 Then
                damageAmount = damageAmount * 0
            End If
            If Me.statusEffects(9) > 0 Then
                damageAmount = damageAmount * 1.2
            End If
            If Me.statusEffects(10) > 0 Then
                damageAmount = damageAmount * 1.5
            End If
            If Me.statusEffects(11) > 0 Then
                damageAmount = damageAmount * 2
            End If

            Me.currentHealth -= damageAmount

            If Me.currentHealth < 0 Then
                Me.currentHealth = 0
            End If

            Console.WriteLine(Me.firstName & " took " & healthBeforeHit - Me.currentHealth & " damage!")
            CheckAlive()
        End Sub

        Public Sub HealAlly(healAmount As Integer, toAlly As Integer)
            playerarray(toAlly).currentHealth += healAmount
            If playerarray(toAlly).currentHealth > playerarray(toAlly).maxHealth Then
                playerarray(toAlly).currentHealth = playerarray(toAlly).maxHealth
            End If
        End Sub

        Public Function ChooseTargetEnemy(randomEnemy As Boolean)

            If Not AnyEnemiesAlive() Then
                Return 0
            End If

            Dim chosenTarget As Integer

            If Me.statusEffects(12) > 0 Then
                Select Case Math.Ceiling(Rnd() * 2)
                    Case 1 'random enemy
                        randomEnemy = True
                    Case 2 'random player
                        Return -1
                        End
                End Select
            End If

            If randomEnemy = False Then
                Do
                    BlankLine()
                    Console.WriteLine("Choose target: ")
                    For x = 0 To enemyList.Count - 1
                        If enemyList(x).alive Then
                            BlankLine()
                            Console.WriteLine(x + 1 & ": " & enemyList(x).name)
                        End If
                    Next
                    BlankLine()
                    Console.WriteLine("============")
                    BlankLine()
                    Console.Write("Input target: ")
                    chosenTarget = getCleanNumericalInput()
                    If chosenTarget > enemyList.Count Or chosenTarget <= 0 Then
                        chosenTarget = -1
                    Else
                        chosenTarget -= 1

                        If enemyList(chosenTarget).alive = False Then
                            chosenTarget = -1
                        End If
                    End If
                    RefreshCombatUI()
                Loop Until chosenTarget <> -1
            Else
                Do
                    chosenTarget = Math.Ceiling(Rnd() * enemyList.Count) - 1
                Loop While enemyList(chosenTarget).alive = False
            End If

            RefreshCombatUI()

            Return chosenTarget
        End Function

        Public Function ChooseTargetPlayer(randomPlayer As Boolean)
            Dim chosenTarget As Integer
            If randomPlayer = False Then
                Do
                    Console.WriteLine("Choose target: ")
                    For x = 1 To 4
                        If playerarray(x).alive Then
                            BlankLine()
                            Console.WriteLine(x & ": " & playerarray(x).firstName)
                        End If
                    Next
                    BlankLine()
                    chosenTarget = getCleanNumericalInput()

                    If chosenTarget > 4 Then
                        chosenTarget = -1
                    Else

                        If playerarray(chosenTarget).alive = False Then
                            chosenTarget = -1
                        End If
                    End If

                Loop Until chosenTarget <> -1
            Else
                Do
                    chosenTarget = Math.Ceiling(Rnd() * playerarray.Count - 1)
                Loop While playerarray(chosenTarget).alive = False
            End If

            Return chosenTarget
        End Function

        Public Sub GainStatusEffect(statusEffect As Integer, amount As Integer)
            Me.statusEffects(statusEffect) += amount
        End Sub

        Public Function ChooseXMana()
            Dim chosenAmount As Integer

            Do
                Console.Write("How much Quintessence are you paying for X?: ")
                chosenAmount = getCleanNumericalInput()

                If chosenAmount > Me.currentQu Then
                    chosenAmount = -1
                End If

                If chosenAmount = -1 Then
                    Console.WriteLine("Please insert a valid amount of Quintessence (that you can afford!).")
                Else
                    Me.currentQu -= chosenAmount
                End If

            Loop While chosenAmount = -1

            Return chosenAmount

        End Function

        Public Sub DebugRegistrySetup()
            'For x = 1 To 2
            '    AddActionToArea(17, 1)
            '    AddActionToArea(5, 1)
            'Next
            'AddActionToArea(1, 1)

            For x = 1 To 10
                AddActionToArea(1, 1)
            Next

            ShuffleArea(1)
        End Sub 'ONLY USED FOR DEBUG

    End Class

    Public Class Enemy
        Public enemyIDRelativeToEncounter As Integer
        Public alive As Boolean
        Public maxHP As Integer
        Public currentHP As Integer
        Public statusEffects(512) As Integer
        Public temporalStatusEffects(512) As Integer
        Public AIType As Integer
        Public subtype As Integer
        Public enemyTier As Integer '1 = enemy, 2 = miniboss, 3 = denizen, 4 = non-denizen boss

        Public name As String
        Public hasTitle As Boolean
        Public title As String
        Public gristDrop As Integer

        Public ogreCounter As Integer
        Public thesisSpawnCounter As Integer

        Public Sub New(enemyID As Integer, AITypeInt As Integer, subtypeInt As Integer)
            enemyIDRelativeToEncounter = enemyID
            Me.AIType = AITypeInt
            Me.alive = True
            Me.subtype = subtypeInt
            Me.hasTitle = False

            Select Case AIType
                Case 0 'Imps
                    Me.maxHP = 10
                    Me.currentHP = 10
                    Me.enemyTier = 1
                    Me.name = "Imp"

                    Select Case subtype
                        Case 0 'Shale Imp
                            Me.name = "Shale Imp"
                            Me.gristDrop = Math.Ceiling(Rnd() * 20) + 40 '41-60
                        Case 1 'Chalk Imp
                            Me.name = "Chalk Imp"
                            Me.maxHP += -2
                            Me.currentHP += -2
                            Me.gristDrop = Math.Ceiling(Rnd() * 10) + 45 '46-55
                        Case 2 'Mercury Imp
                            Me.name = "Mercury Imp"
                            Me.maxHP += 3
                            Me.currentHP += 3
                            Me.gristDrop = Math.Ceiling(Rnd() * 10) + 55 '56-65
                        Case 3 'Cobalt Imp
                            Me.name = "Cobalt Imp"
                            Me.maxHP += 7
                            Me.currentHP += 7
                            Me.gristDrop = Math.Ceiling(Rnd() * 10) + 65 '66-75
                        Case 4 'Marble Imp
                            Me.name = "Marble Imp"
                            Me.maxHP += 3
                            Me.currentHP += 3
                            Me.gristDrop = Math.Ceiling(Rnd() * 10) + 60 '61-70
                        Case 5 'Amber Imp
                            Me.name = "Amber Imp"
                            Me.maxHP += 1
                            Me.currentHP += 1
                            Me.gristDrop = Math.Ceiling(Rnd() * 10) + 90 '91-100
                        Case 6 'Rust Imp
                            Me.name = "Rust Imp"
                            Me.maxHP += 15
                            Me.currentHP += 15
                            Me.gristDrop = Math.Ceiling(Rnd() * 10) + 90 '91-100
                            Me.statusEffects(15) = 3
                        Case 7 'Uranium Imp
                            Me.name = "Uranium Imp"
                            Me.maxHP += 10
                            Me.currentHP += 10
                            Me.gristDrop = Math.Ceiling(Rnd() * 10) + 140 '141-150
                        Case 99
                            Me.name = "Error Imp"
                            Me.maxHP = 1111
                            Me.currentHP = 1111
                            Me.gristDrop = 1111
                    End Select
                Case 1 'Ogre
                    Me.maxHP = 30
                    Me.currentHP = 30
                    Me.enemyTier = 1
                    Me.name = "Ogre"
                    Me.ogreCounter = 0

                    Select Case subtype
                        Case 0 'Crude Ogre
                            Me.name = "Crude Ogre"
                            Me.gristDrop = 150
                        Case 1 'Lime Ogre
                            Me.name = "Lime Ogre"
                            Me.maxHP += -5
                            Me.currentHP += -5
                            Me.gristDrop = 150
                        Case 2 'Mercury Ogre
                            Me.name = "Mercury Ogre"
                            Me.maxHP += 2
                            Me.currentHP += 2
                            Me.gristDrop = 200
                        Case 3 'Cobalt Ogre
                            Me.name = "Cobalt Ogre"
                            Me.maxHP += 8
                            Me.currentHP += 8
                            Me.gristDrop = 200
                        Case 4 'Marble Ogre
                            Me.name = "Marble Ogre"
                            Me.maxHP += 5
                            Me.currentHP += 5
                            Me.gristDrop = 200
                        Case 5 'Sulfur Ogre
                            Me.name = "Sulfur Ogre"
                            Me.maxHP += 2
                            Me.currentHP += 2
                            Me.gristDrop = 250
                        Case 6 'Rust Ogre
                            Me.name = "Rust Ogre"
                            Me.maxHP += 20
                            Me.currentHP += 20
                            Me.gristDrop = 250
                            Me.statusEffects(15) = 4
                        Case 7 'Uranium Ogre
                            Me.name = "Uranium Ogre"
                            Me.maxHP += 10
                            Me.currentHP += 10
                            Me.gristDrop = 250
                    End Select
                Case 2 'Basilisk
                    Me.maxHP = 20
                    Me.currentHP = 20
                    Me.enemyTier = 1
                    Me.name = "Basilisk"

                    Select Case subtype
                        Case 0 'Tar Basilisk
                            Me.name = "Tar Basilisk"
                            Me.gristDrop = 100
                        Case 1 'Agate Basilisk
                            Me.name = "Agate Basilisk"
                            Me.maxHP += -4
                            Me.currentHP += -4
                            Me.gristDrop = 100
                        Case 2 'Lead Basilisk
                            Me.name = "Lead Basilisk"
                            Me.maxHP += -1
                            Me.currentHP += -1
                            Me.gristDrop = 150
                        Case 3 'Lazurite Basilisk
                            Me.name = "Lazurite Basilisk"
                            Me.maxHP += 7
                            Me.currentHP += 7
                            Me.gristDrop = 150
                        Case 4 'Marble Basilisk
                            Me.name = "Marble Basilisk"
                            Me.maxHP += 4
                            Me.currentHP += 4
                            Me.gristDrop = 150
                        Case 5 'Pyrite Basilisk
                            Me.name = "Pyrite Basilisk"
                            Me.maxHP += 2
                            Me.currentHP += 2
                            Me.gristDrop = 200
                        Case 6 'Zinc Basilisk
                            Me.name = "Zinc Basilisk"
                            Me.maxHP += 15
                            Me.currentHP += 15
                            Me.gristDrop = 200
                            Me.statusEffects(15) = 4
                        Case 7 'Uranium Basilisk
                            Me.name = "Uranium Basilisk"
                            Me.maxHP += 8
                            Me.currentHP += 8
                            Me.gristDrop = 200
                    End Select

                Case 3 'Lich
                    Me.maxHP = 25
                    Me.currentHP = 25
                    Me.enemyTier = 1
                    Me.name = "Lich"

                    Select Case subtype
                        Case 0 'Goethite Lich
                            Me.name = "Goethite Lich"
                            Me.gristDrop = 150
                        Case 1 'Caulk Lich
                            Me.name = "Caulk Lich"
                            Me.maxHP += -10
                            Me.currentHP += -10
                            Me.gristDrop = 150
                        Case 2 'Ice Lich
                            Me.name = "Ice Lich"
                            Me.maxHP += 3
                            Me.currentHP += 3
                            Me.gristDrop = 175
                        Case 3 'Fluorite Lich
                            Me.name = "Fluorite Lich"
                            Me.maxHP += 6
                            Me.currentHP += 6
                            Me.gristDrop = 175
                        Case 4 'Pearl Lich
                            Me.name = "Pearl Lich"
                            Me.maxHP += 4
                            Me.currentHP += 4
                            Me.gristDrop = 175
                        Case 5 'Pyrite Lich
                            Me.name = "Pyrite Lich"
                            Me.maxHP += 2
                            Me.currentHP += 2
                            Me.gristDrop = 225
                        Case 6 'Fougèrite Lich
                            Me.name = "Fougèrite Lich"
                            Me.maxHP += 20
                            Me.currentHP += 20
                            Me.gristDrop = 225
                            Me.statusEffects(14) = 10
                        Case 7 'Plutonium Lich
                            Me.name = "Plutonium Lich"
                            Me.maxHP += 5
                            Me.currentHP += 5
                            Me.gristDrop = 225
                    End Select

                Case 4 'Giclops
                    Me.maxHP = 50
                    Me.currentHP = 50
                    Me.enemyTier = 1
                    Me.name = "Giclops"

                    Select Case subtype
                        Case 0 'Copper Giclops
                            Me.name = "Copper Giclops"
                            Me.gristDrop = 100
                        Case 1 'Grossular Giclops
                            Me.name = "Grossular Giclops"
                            Me.maxHP += -10
                            Me.currentHP += -10
                            Me.gristDrop = 100
                        Case 2 'Mercury Giclops
                            Me.name = "Mercury Giclops"
                            Me.maxHP += -2
                            Me.currentHP += -2
                            Me.gristDrop = 150
                        Case 3 'Covellite Giclops
                            Me.name = "Covellite Giclops"
                            Me.maxHP += 10
                            Me.currentHP += 10
                            Me.gristDrop = 150
                        Case 4 'Marble Giclops
                            Me.name = "Marble Giclops"
                            Me.maxHP += 5
                            Me.currentHP += 5
                            Me.gristDrop = 150
                        Case 5 'Ruby Giclops
                            Me.name = "Ruby Giclops"
                            Me.maxHP += 3
                            Me.currentHP += 3
                            Me.gristDrop = 200
                        Case 6 'Rust Giclops
                            Me.name = "Rust Giclops"
                            Me.maxHP += 20
                            Me.currentHP += 20
                            Me.gristDrop = 200
                            Me.statusEffects(15) = 7
                        Case 7 'Uranium Giclops
                            Me.name = "Uranium Giclops"
                            Me.maxHP += 10
                            Me.currentHP += 10
                            Me.gristDrop = 200
                    End Select

                Case 5 'Minibosses

                    Me.enemyTier = 2

                Case 6 'Denizens

                    Me.enemyTier = 3
                    Me.gristDrop = 612413

                    Select Case subtype
                        Case 1

                        Case 2

                        Case 3

                        Case 4

                        Case 5 'Thesis, Creation Incarnate
                            Me.maxHP = 200
                            Me.currentHP = 200
                            Me.name = "Thesis"
                            Me.title = "Creation Incarnate"
                            Me.hasTitle = True
                            Me.thesisSpawnCounter = 0
                    End Select

            End Select
        End Sub

        Public Sub TakeTurn()
            Dim rngInt As Integer = 0
            Dim rngInt2 As Integer = 0
            Dim rngInt3 As Integer = 0

            Console.WriteLine("[" & Me.name & "'s turn!]")

            Select Case AIType
                Case 0 'Imps
                    Dim damageToDeal As Integer

                    Select Case subtype
                        Case 0 'Shale Imp
                            damageToDeal = 4 'change these depending on subtype
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 1 'Chalk Imp
                            damageToDeal = 5
                            rngInt = Math.Ceiling(Rnd() * 90)
                        Case 2 'Mercury Imp
                            damageToDeal = 3
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 3 'Cobalt Imp
                            damageToDeal = 4
                            Do
                                rngInt = Math.Ceiling(Rnd() * 100)
                            Loop While rngInt > 80 And rngInt < 90
                        Case 4 'Marble Imp
                            damageToDeal = 5
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 5 'Amber Imp
                            damageToDeal = 7
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 6 'Rust Imp
                            damageToDeal = 4
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 7 'Uranium Imp
                            damageToDeal = 4
                            rngInt = Math.Ceiling(Rnd() * 100)
                    End Select


                    'Possible Actions
                    '70% - Attack for full damage towards 1 player.
                    '10% - Idle
                    '10% - Hurt self and gain Confuse 2-4.
                    '10% - Gain Sinew 2.

                    If rngInt <= 70 Then '40% - Attack for full damage towards 1 player.
                        rngInt = Math.Ceiling(Rnd() * 6)
                        Select Case rngInt
                            Case 1
                                Console.WriteLine(Me.name & " scratches at " & playerarray(1).firstName & "!")
                            Case 2
                                Console.WriteLine(Me.name & " pokes " & playerarray(1).firstName & "!")
                            Case 3
                                Console.WriteLine(Me.name & " lashes out at " & playerarray(1).firstName & "!")
                            Case 4
                                Console.WriteLine(Me.name & " bonks " & playerarray(1).firstName & "!")
                            Case 5
                                Console.WriteLine(Me.name & " slaps " & playerarray(1).firstName & "!")
                            Case Else
                                Console.WriteLine(Me.name & " makes some perculiar limb flailings vaguely in " & playerarray(1).firstName & "'s direction!")
                        End Select

                        AttackPlayer(damageToDeal, 1)

                        If Me.subtype = 2 Then
                            playerarray(1).GainStatusEffect(14, 2)
                        ElseIf Me.subtype = 7 Then
                            playerarray(1).GainStatusEffect(15, 1)
                        End If
                    ElseIf rngInt <= 80 Then '10% - Idle
                        rngInt = Math.Ceiling(Rnd() * 4)
                        Select Case rngInt
                            Case 1
                                Console.WriteLine(Me.name & " yells something incomprehensible, but surely imflammatory in nature.")
                            Case 2
                                Console.WriteLine(Me.name & " chortles to themselves.")
                            Case 3
                                Console.WriteLine(Me.name & " takes a breather.")
                            Case Else
                                Console.WriteLine(Me.name & " giggles to themselves.")
                        End Select
                    ElseIf rngInt <= 90 Then '10% - Hurt self and gain Confuse 2-4.
                        TakeDamage(3)
                        CheckIfAlive()
                        Me.statusEffects(12) += Math.Ceiling(Rnd() * 2) + 2
                        If Me.alive Then
                            Console.WriteLine(Me.name & " trips over, taking 3 damage and confusing themselves.")
                        Else
                            Console.WriteLine(Me.name & " trips over and dies.")
                        End If
                    Else '10% - Gain Sinew 2.
                        Me.statusEffects(1) += 2
                        Console.WriteLine(Me.name & " prepares to throw down!")
                    End If


                Case 1 'Ogres

                    Dim damageToDeal As Integer = 9

                    Select Case subtype
                        Case 0 'Crude Ogre
                            rngInt = Math.Ceiling(Rnd() * 100) - Me.ogreCounter
                        Case 1 'Lime Ogre
                            damageToDeal += 1
                            rngInt = Math.Ceiling(Rnd() * 80) - Me.ogreCounter
                        Case 2 'Mercury Ogre
                            damageToDeal += -2
                            rngInt = Math.Ceiling(Rnd() * 100) - Me.ogreCounter
                        Case 3 'Cobalt Ogre
                            rngInt = Math.Ceiling(Rnd() * 100) - Me.ogreCounter * 3
                        Case 4 'Marble Ogre
                            damageToDeal += 1
                            rngInt = Math.Ceiling(Rnd() * 100) - Me.ogreCounter
                        Case 5 'Sulfur Ogre
                            damageToDeal += 3
                            rngInt = Math.Ceiling(Rnd() * 100) - Me.ogreCounter
                        Case 6 'Rust Ogre
                            rngInt = Math.Ceiling(Rnd() * 100) - Me.ogreCounter
                        Case 7 'Uranium Ogre
                            rngInt = Math.Ceiling(Rnd() * 100) - Me.ogreCounter
                    End Select

                    If rngInt < 0 Then
                        rngInt = 0
                    End If
                    Me.ogreCounter += 1

                    'Possible Actions
                    '1-30 - Deal 9 damage to player
                    '31-80 - Idle
                    '80-90 - Gain 5 Potency
                    '91-95 - Kill another enemy and deal damage equal to thier health
                    '96-100 - Give player Lackadaisical 1

                    If rngInt <= 30 Then
                        Console.WriteLine(Me.name & " punches " & playerarray(1).firstName & " in the face!")
                        AttackPlayer(damageToDeal, 1)

                        If Me.subtype = 2 Then
                            playerarray(1).GainStatusEffect(14, 3)
                        ElseIf Me.subtype = 7 Then
                            playerarray(1).GainStatusEffect(15, 1)
                        End If
                    ElseIf rngInt <= 80 Then
                        Console.WriteLine(Me.name & " is distracted; surely by something very important and cool.")
                    ElseIf rngInt <= 90 Then
                        Me.statusEffects(0) += 5
                        Console.WriteLine(Me.name & " gained a boost of confidence!")
                    ElseIf rngInt <= 95 Then
                        Dim loopPreventionCounter As Integer = 0
                        Do
                            loopPreventionCounter += 1
                            rngInt2 = Math.Ceiling(Rnd() * enemyList.Count)
                        Loop Until enemyList(rngInt2 - 1).name <> Me.name And enemyList(rngInt2 - 1).alive = True Or loopPreventionCounter > 100

                        If loopPreventionCounter > 100 Then
                            Console.WriteLine(Me.name & " is distracted; probably by something lame.")
                        Else
                            AttackPlayer(enemyList(rngInt2 - 1).currentHP * 0.8, 1)
                            Console.WriteLine(Me.name & " picks up " & enemyList(rngInt2 - 1).name & " and throws them at " & playerarray(1).firstName & ", dealing " & enemyList(rngInt2 - 1).currentHP * 0.8 & " damage!")
                            Console.WriteLine(enemyList(rngInt2 - 1).name & " takes 15 damage!")
                            enemyList(rngInt2 - 1).TakeDamage(15)
                        End If
                    Else
                        playerarray(1).GainStatusEffect(4, 2)
                        Console.WriteLine(Me.name & " stomps the ground below them, making " & playerarray(1).firstName & " lose their footing!")
                    End If

                Case 2 'Basilisks

                    Dim damageToDeal As Integer = 4

                    Select Case subtype
                        Case 0 'Tar Basilisk
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 1 'Agate Basilisk
                            damageToDeal += 1
                            rngInt = Math.Ceiling(Rnd() * 90)
                        Case 2 'Lead Basilisk
                            damageToDeal += -1
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 3 'Lazurite Basilisk
                            Do
                                rngInt = Math.Ceiling(Rnd() * 100)
                            Loop While rngInt >= 61 And rngInt <= 80
                        Case 4 'Marble Basilisk
                            damageToDeal += 1
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 5 'Pyrite Basilisk
                            damageToDeal += 3
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 6 'Zinc Basilisk
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 7 'Uranium Basilisk
                            rngInt = Math.Ceiling(Rnd() * 100)
                    End Select

                    'Possible Actions
                    '1-20 - Inflict 3 poison
                    '21-60 - Deal 4 damage
                    '61-80 - Idle
                    '81-90 - Heal for 3-5 health
                    '91-95 - Inflict Susceptible 3
                    '96-100 - Gain Inured 3

                    If rngInt <= 20 Then '1-20 - Inflict 3 poison

                        If Me.subtype = 2 Then
                            Console.WriteLine(Me.name & " spits paralyzing venom at " & playerarray(1).firstName & "!")
                            playerarray(1).GainStatusEffect(3, 3)
                        ElseIf Me.subtype = 7 Then
                            Console.WriteLine(Me.name & " spits blueish-black venom at " & playerarray(1).firstName & "!")
                            Console.WriteLine("It looks like eight-ball fluid...")
                            playerarray(1).GainStatusEffect(3, 3)
                        Else
                            Console.WriteLine(Me.name & " spits poisonous venom at " & playerarray(1).firstName & "!")
                            playerarray(1).GainStatusEffect(14, 3)
                        End If

                    ElseIf rngInt <= 60 Then '21-60 - Deal 4 damage
                        Console.WriteLine(Me.name & " swipes their recently sharpened claws at " & playerarray(1).firstName & "!")
                        AttackPlayer(damageToDeal, 1)
                    ElseIf rngInt <= 80 Then '61-80 - Idle
                        Console.WriteLine(Me.name & " attempts to slash at " & playerarray(1).firstName & ", but misses!")
                    ElseIf rngInt <= 90 Then '81-90 - Heal for 3-5 health
                        rngInt = Math.Ceiling(Rnd() * 3) + 2
                        Console.WriteLine(Me.name & " takes a second to breath, and regenerates a missing limb or two back.")
                        Console.WriteLine(Me.name & " heals " & rngInt & " health!")
                        TakeDamage(-rngInt)
                    ElseIf rngInt <= 95 Then '91-95 - Inflict Susceptible 3
                        Console.WriteLine(Me.name & " trips " & playerarray(1).firstName & " up with a swipe of thier tail!")
                        playerarray(1).GainStatusEffect(10, 3)
                    Else '96-100 - Gain Inured 3
                        Console.WriteLine(Me.name & "'s scales harden up; it's prepared to take a wild walloping!")
                        Me.statusEffects(7) += 3
                    End If

                Case 3 'Liches

                    Dim damageToDeal As Integer = 0
                    Dim noOfRolls As Integer = 1
                    Dim successChance As Integer = 20

                    Select Case subtype
                        Case 0 'Goethite Lich
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 1 'Caulk Lich
                            rngInt = Math.Ceiling(Rnd() * 100)
                            successChance = 10
                            damageToDeal = 3
                        Case 2 'Ice Lich
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 3 'Fluorite Lich
                            rngInt = Math.Ceiling(Rnd() * 100)
                            noOfRolls = 2
                        Case 4 'Pearl Lich
                            damageToDeal = 3
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 5 'Pyrite Lich
                            rngInt = Math.Ceiling(Rnd() * 100)
                            successChance = 50
                        Case 6 'Fougèrite Lich
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 7 'Plutonium Lich
                            rngInt = Math.Ceiling(Rnd() * 100)
                            successChance = 10
                            noOfRolls = 10
                    End Select

                    For x = 1 To noOfRolls
                        Select Case x
                            Case 1
                                Console.WriteLine(Me.name & " rolls for their attack...")
                            Case 2
                                Console.WriteLine(Me.name & " rolls for their second attack...")
                            Case 3
                                Console.WriteLine(Me.name & " rolls for their third attack...")
                            Case 4
                                Console.WriteLine(Me.name & " rolls for their fourth attack...")
                            Case 5
                                Console.WriteLine(Me.name & " rolls for their fifth attack...")
                            Case 6
                                Console.WriteLine(Me.name & " rolls for their sixth attack...")
                            Case 7
                                Console.WriteLine(Me.name & " rolls for their seventh attack...")
                            Case 8
                                Console.WriteLine(Me.name & " rolls for their eighth attack...")
                            Case 9
                                Console.WriteLine(Me.name & " rolls for their ninth attack...")
                            Case 10
                                Console.WriteLine(Me.name & " rolls for their final, tenth attack...")
                        End Select

                        If Me.subtype = 7 Then
                            rngInt = Math.Ceiling(Rnd() * 100)
                        End If

                        If rngInt <= successChance Then
                            Console.WriteLine("They succeed!")

                            Select Case Math.Ceiling(Rnd() * 10)
                                Case 1
                                    Console.WriteLine("They cast [Tetrachromatose]!")
                                    Select Case Math.Ceiling(Rnd() * 4)
                                        Case 1
                                            Console.BackgroundColor = ConsoleColor.DarkBlue
                                            Console.WriteLine("The surroundings quickly fill with a blueish, drowning hue.")
                                        Case 2
                                            Console.BackgroundColor = ConsoleColor.DarkRed
                                            Console.WriteLine("The surroundings quickly fill with an ominous red hue.")
                                        Case 3
                                            Console.BackgroundColor = ConsoleColor.DarkYellow
                                            Console.ForegroundColor = ConsoleColor.Black
                                            Console.WriteLine("The surroundings quickly fill with an unfortunate yellow hue.")
                                        Case 4
                                            Console.BackgroundColor = ConsoleColor.DarkGreen
                                            Console.WriteLine("The surroundings quickly fill with a haunting green hue.")
                                    End Select
                                    AttackPlayer(Math.Ceiling(Rnd() * 5) + damageToDeal, 1)
                                Case 2
                                    playerarray(1).CopyAreaIntoAnotherArea(-1, 1)
                                    playerarray(1).ShuffleArea(1)
                                    Console.WriteLine("They cast [Deckplicate]!")
                                    Console.WriteLine(playerarray(1).firstName & "'s deck shuffled an extra copy of itself into itself!")
                                Case 3
                                    For i = 1 To 3
                                        playerarray(1).AddActionToArea(0, 1)
                                    Next
                                    playerarray(1).ShuffleArea(1)
                                    Console.WriteLine("They cast [Absolabstinence]!")
                                    Console.WriteLine(playerarray(1).firstName & "'s deck gained a couple more [Abstain]s. Neat.")
                                Case 4
                                    Console.WriteLine("They cast [D20]!")
                                    Console.WriteLine("The D stands for Discomfort.")
                                    AttackPlayer(Math.Ceiling(Rnd() * 20) + damageToDeal, 1)
                                Case 5
                                    Console.WriteLine("They cast [Correct Twice a Day]!")
                                    Console.WriteLine("Time seems to come to a halt in a localised area around " & playerarray(1).firstName & "!")
                                    playerarray(1).temporalStatusEffects(0) += 4
                                Case 6
                                    Console.WriteLine("They cast [Rewindup Kick]!")
                                    Console.WriteLine("The temporal position " & playerarray(1).firstName & " exists in is not on talking terms with the one their opponents are in.")
                                    Console.WriteLine("The " & Me.name & " doesn't care about this though, and kicks " & playerarray(1).firstName & " square in the face. Somehow.")
                                    playerarray(1).temporalStatusEffects(1) += 2
                                    AttackPlayer(Math.Ceiling(Rnd() * 3) + 4 + damageToDeal, 1)
                                Case 7
                                    Console.WriteLine("They cast [Inchronocrease-&-cripple]!")
                                    Console.WriteLine("Time begins to fly past " & playerarray(1).firstName & ", as well as a myriad of angry fists.")
                                    playerarray(1).temporalStatusEffects(2) += 4
                                    For i = 1 To (Math.Ceiling(Rnd() * 8) + 2)
                                        AttackPlayer(Math.Ceiling(Rnd() * 2) + damageToDeal, 1)
                                    Next
                                Case 8
                                    Console.WriteLine("They cast [Nerf Hammer]!")
                                    Console.WriteLine(playerarray(1).firstName & " feels drained...")
                                    playerarray(1).GainStatusEffect(5, 2)
                                Case 9
                                    Console.WriteLine("They cast [Ultra Violence]!")
                                    Console.WriteLine(playerarray(1).firstName & " feels like that might be a reference to something.")
                                    playerarray(1).GainStatusEffect(10, 2)
                                Case 10
                                    Console.WriteLine("They cast [Heart of the Cards]!")
                                    Console.WriteLine(playerarray(1).firstName & "'s hand is forced to draw, and then destroy, the top action from their Action Registry...")
                                    If playerarray(1).actionRegistry.Count <> 0 Then
                                        playerarray(1).ResolveAction(playerarray(1).actionRegistry(0), False, True)
                                        playerarray(1).actionRegistry.RemoveAt(0)
                                    Else
                                        Console.WriteLine("However, as " & playerarray(1).firstName & "'s Action Registry is currently empty, the spell misses and fizzles out.")
                                        Console.WriteLine("How embarrassing...")
                                    End If
                            End Select
                        Else
                            Console.WriteLine("They fail...")
                        End If
                    Next

                Case 4 'Giclops

                    Dim damageToDeal As Integer = 0

                    Select Case subtype
                        Case 0 'Copper Giclops
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 1 'Grossular Giclops
                            rngInt = Math.Ceiling(Rnd() * 90)
                            damageToDeal = 2
                        Case 2 'Mercury Giclops
                            damageToDeal = -7
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 3 'Covellite Giclops
                            rngInt = Math.Ceiling(Rnd() * 80) + 20
                        Case 4 'Marble Giclops
                            rngInt = Math.Ceiling(Rnd() * 100)
                            damageToDeal = 1
                        Case 5 'Ruby Giclops
                            rngInt = Math.Ceiling(Rnd() * 100)
                            damageToDeal = 2
                        Case 6 'Rust Giclops
                            rngInt = Math.Ceiling(Rnd() * 100)
                        Case 7 'Uranium Giclops
                            rngInt = Math.Ceiling(Rnd() * 100)
                            damageToDeal = 5
                    End Select

                    'Possible Actions
                    '1-60 - Idle
                    '61-90 - Deal 10-15 damage
                    '91-100 - Add 1 abstain to player's hand

                    If rngInt <= 60 Then
                        Console.WriteLine(Me.name & " can't seem to figure out where to punch...")
                    ElseIf rngInt <= 90 Then
                        Console.WriteLine(Me.name & " decks " & playerarray(1).firstName & " in the stomach!")
                        AttackPlayer(Math.Ceiling(Rnd() * 6) + 9 + damageToDeal, 1)
                        Console.WriteLine("Ouch.")
                    Else
                        playerarray(1).AddActionToArea(0, 1)
                        Console.WriteLine(Me.name & " roars in " & playerarray(1).firstName & "'s face!")
                        Console.WriteLine("A new pair of colourful undergarments may or may not be in order...")
                    End If

                Case 5 'Minibosses

                Case 6 'Denizens

                    Select Case subtype
                        Case 5 'Thesis, Creation Incarnate

                            rngInt = Math.Ceiling(Rnd() * 100)
                            If Me.currentHP <= 40 And rngInt <= 15 Then
                                rngInt = 30
                            End If

                            If rngInt <= 15 Then
                                Dim spawnEnemy As New Enemy(2 + thesisSpawnCounter, Math.Ceiling(Rnd() * 5) - 1, Math.Ceiling(Rnd() * 8) - 1)
                                enemyList.Add(spawnEnemy)
                                Console.WriteLine(Me.name & " creates a new being to fight alongside it from it's own matter, taking 40 damage in the process!")
                                TakeDamage(40)
                            ElseIf rngInt <= 30 Then
                                Console.WriteLine(Me.name & " does nothing, but it's the most powerful nothing you've ever seen.")
                                AttackPlayer(10, 1)
                            Else
                                Console.WriteLine("The very canvas upon which you are drawn seems to harbour nothing but silent distain for you.")
                                Console.WriteLine("Thesis toils.")
                            End If

                    End Select

            End Select

            BlankLine()
            EndOfTurn()
        End Sub 'NEEDS UPDATING FOR A SINGLE PLAYER GAME

        Public Sub AttackPlayer(damageAmount As Integer, target As Integer)
            'target legend
            '1 - 4: players
            '0: random player
            '5: all players

            If Me.statusEffects(0) > 0 Then
                damageAmount = damageAmount * 1.2
            End If
            If Me.statusEffects(1) > 0 Then
                damageAmount = damageAmount * 1.5
            End If
            If Me.statusEffects(2) > 0 Then
                damageAmount = damageAmount * 2
            End If
            If Me.statusEffects(3) > 0 Then
                damageAmount = damageAmount * 0.8
            End If
            If Me.statusEffects(4) > 0 Then
                damageAmount = damageAmount * 0.5
            End If
            If Me.statusEffects(5) > 0 Then
                damageAmount = damageAmount * 0
            End If

            If target = 0 Then
                Do
                    target = Math.Ceiling(Rnd() * 4)
                Loop Until playerarray(target).alive
            End If

            'check if enemy is confused
            If Me.statusEffects(12) > 0 Then
                target = Math.Ceiling(Rnd() * enemyList.Count) - 1
                Console.WriteLine("...but they miss! Instead, " & enemyList(target).name & " takes " & damageAmount & "damage!")
                enemyList(target).TakeDamage(damageAmount)
                target = 0
            End If

            If target > 0 And target < 5 Then
                playerarray(target).TakeDamage(damageAmount)
            End If
        End Sub 'NEEDS UPDATING FOR A SINGLE PLAYER GAME

        Public Sub TakeDamage(damageAmount As Double)
            Dim healthBeforeHit As Integer = Me.currentHP
            If Me.statusEffects(6) > 0 Then
                damageAmount = damageAmount * 0.8
            End If
            If Me.statusEffects(7) > 0 Then
                damageAmount = damageAmount * 0.5
            End If
            If Me.statusEffects(8) > 0 Then
                damageAmount = damageAmount * 0
            End If
            If Me.statusEffects(9) > 0 Then
                damageAmount = damageAmount * 1.2
            End If
            If Me.statusEffects(10) > 0 Then
                damageAmount = damageAmount * 1.5
            End If
            If Me.statusEffects(11) > 0 Then
                damageAmount = damageAmount * 2
            End If

            Me.currentHP -= damageAmount

            If Me.currentHP < 0 Then
                Me.currentHP = 0
            End If

            CheckIfAlive()
        End Sub

        Public Sub CheckIfAlive()
            If currentHP <= 0 Then
                If Me.alive = True Then
                    Me.alive = False
                    Die()
                End If
            Else
                Me.alive = True
            End If
        End Sub

        Public Sub Die()
            Console.WriteLine("+" & Me.gristDrop & " Grist gained!")
            gristCache += Me.gristDrop
        End Sub

        Public Sub EndOfTurn() 'ADD: maybe move this elsewhere so that enemies lose status effects at a more sensible time, instead of right after gaining them.
            If statusEffects(14) > 0 Then
                TakeDamage(statusEffects(14))
            End If
            If statusEffects(15) > 0 Then
                TakeDamage(statusEffects(15))
            End If

            Dim statusEffectChange As Integer = -1
            'ADD: Check for temporal status effects

            For x = 1 To 512 'Lowering all status effects by determined amount
                If x <> 15 Then
                    Me.statusEffects(x) += statusEffectChange
                    If Me.statusEffects(x) < 0 Then
                        Me.statusEffects(x) = 0
                    End If
                End If
            Next

            For x = 1 To 512 'Lowering all temporal status effects by 1
                Me.temporalStatusEffects(x) -= 1
                If Me.temporalStatusEffects(x) < 0 Then
                    Me.temporalStatusEffects(x) = 0
                End If
            Next
        End Sub

    End Class

    Public Class EnemyEncounter
        'these two values combined determine the pool that the enemies will be drawn from
        Public encounterType As Integer '0 = overworld random encounter, 1 = dungeon random encounter, 2 = miniboss, 3 = denizen, 4 = non-denizen boss
        Public dungeonFloor As Integer
        Public battleOver As Boolean
        Public victoryHad As Boolean
        Public localEnemyList As New List(Of Enemy)

        'Public playerList As New List(Of Player)
        Public battleMusic As String
        Public Sub New(encounterTypeInt As Integer, dungeonFloorInt As Integer)
            Me.battleOver = False
            Me.victoryHad = False
            Me.encounterType = encounterTypeInt
            Me.dungeonFloor = dungeonFloorInt
            localEnemyList.Clear()
            InitializeEncounter()
        End Sub

        Public Sub EncounterInfo()
            Console.WriteLine("ENCOUNTER!!!")
            For x = 0 To localEnemyList.Count - 1
                Console.WriteLine(localEnemyList(x).name)
            Next
            Console.WriteLine("ENCOUNTER OVER...")
        End Sub

        Public Sub InitializeEncounter()
            'Me.encounterType = 1

            'For x = 1 To Me.landID
            'Me.playerList.Add(playerarray(x))
            'Next

            Select Case Me.encounterType
                Case 0 'For testing specific enemy layouts
                    For x = 1 To 1
                        Dim enemy As New Enemy(x, 6, 5)
                        localEnemyList.Add(enemy)
                    Next
                Case 1 'Enemies
                    Select Case Me.dungeonFloor
                        Case 1 'Floor 1 Enemies
                            Select Case Math.Ceiling(Rnd() * 2)
                                Case 1 '2 to 3 easy imps
                                    For x = 1 To Math.Ceiling(Rnd() * 2) + 1
                                        Dim enemy1 As New Enemy(x, 0, Math.Ceiling(Rnd() * 2) - 1)
                                        localEnemyList.Add(enemy1)
                                    Next
                                Case 2 '1 medium imp
                                    Dim enemy1 As New Enemy(1, 0, Math.Ceiling(Rnd() * 3) + 1)
                                    localEnemyList.Add(enemy1)
                            End Select
                        Case 2 'Floor 2 Enemies
                            Select Case Math.Ceiling(Rnd() * 3)
                                Case 1 '1-2 medium imps
                                    For x = 1 To Math.Ceiling(Rnd() * 2)
                                        Dim enemy1 As New Enemy(x, 0, Math.Ceiling(Rnd() * 3) + 1)
                                        localEnemyList.Add(enemy1)
                                    Next
                                Case 2 '2 easy imps & 1 easy ogre
                                    Dim enemy1 As New Enemy(1, 0, Math.Ceiling(Rnd() * 2) - 1)
                                    Dim enemy2 As New Enemy(2, 0, Math.Ceiling(Rnd() * 2) - 1)
                                    Dim enemy3 As New Enemy(3, 1, Math.Ceiling(Rnd() * 2) - 1)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                    localEnemyList.Add(enemy3)
                                Case 3 '1-2 easy basilisks
                                    For x = 1 To Math.Ceiling(Rnd() * 2)
                                        Dim enemy1 As New Enemy(x, 2, Math.Ceiling(Rnd() * 2) - 1)
                                        localEnemyList.Add(enemy1)
                                    Next
                            End Select
                        Case 3 'Floor 3 Enemies
                            Select Case Math.Ceiling(Rnd() * 3)
                                Case 1 '1-2 hard imps & 1 easy ogre
                                    Dim enemy1 As New Enemy(1, 0, Math.Ceiling(Rnd() * 3) + 4)
                                    Dim enemy2 As New Enemy(2, 1, Math.Ceiling(Rnd() * 2) - 1)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                    If Math.Ceiling(Rnd() * 2) = 1 Then
                                        Dim enemy3 As New Enemy(3, 0, Math.Ceiling(Rnd() * 3) + 4)
                                        localEnemyList.Add(enemy3)
                                    End If
                                Case 2 '1-2 medium ogres
                                    For x = 1 To Math.Ceiling(Rnd() * 2)
                                        Dim enemy1 As New Enemy(x, 1, Math.Ceiling(Rnd() * 3) + 1)
                                        localEnemyList.Add(enemy1)
                                    Next
                                Case 3 '1-2 medium basilisks
                                    For x = 1 To Math.Ceiling(Rnd() * 2)
                                        Dim enemy1 As New Enemy(x, 2, Math.Ceiling(Rnd() * 3) + 1)
                                        localEnemyList.Add(enemy1)
                                    Next
                            End Select
                        Case 4 'Floor 4 Enemies
                            Select Case Math.Ceiling(Rnd() * 3)
                                Case 1 '1 hard ogre, 2 medium basilisks
                                    Dim enemy1 As New Enemy(1, 1, Math.Ceiling(Rnd() * 3) + 4)
                                    Dim enemy2 As New Enemy(2, 2, Math.Ceiling(Rnd() * 3) + 1)
                                    Dim enemy3 As New Enemy(3, 2, Math.Ceiling(Rnd() * 3) + 1)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                    localEnemyList.Add(enemy3)
                                Case 2 '2 medium ogres, 1 easy lich
                                    Dim enemy1 As New Enemy(1, 1, Math.Ceiling(Rnd() * 3) + 1)
                                    Dim enemy2 As New Enemy(2, 1, Math.Ceiling(Rnd() * 3) + 1)
                                    Dim enemy3 As New Enemy(3, 3, Math.Ceiling(Rnd() * 2) - 1)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                    localEnemyList.Add(enemy3)
                                Case 3 '1 easy giclops, 2 medium imps, 2 easy imps
                                    Dim enemy1 As New Enemy(1, 4, Math.Ceiling(Rnd() * 2) - 1)
                                    Dim enemy2 As New Enemy(2, 0, Math.Ceiling(Rnd() * 3) + 1)
                                    Dim enemy3 As New Enemy(3, 0, Math.Ceiling(Rnd() * 3) + 1)
                                    Dim enemy4 As New Enemy(4, 0, Math.Ceiling(Rnd() * 2) - 1)
                                    Dim enemy5 As New Enemy(5, 0, Math.Ceiling(Rnd() * 2) - 1)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                    localEnemyList.Add(enemy3)
                                    localEnemyList.Add(enemy4)
                                    localEnemyList.Add(enemy5)
                            End Select
                        Case Else 'Floor 5 Enemies
                            Select Case Math.Ceiling(Rnd() * 6)
                                Case 1 '1 hard giclops
                                    Dim enemy1 As New Enemy(1, 4, Math.Ceiling(Rnd() * 3) + 4)
                                    localEnemyList.Add(enemy1)
                                Case 2 '2 medium giclops
                                    Dim enemy1 As New Enemy(1, 4, Math.Ceiling(Rnd() * 3) + 1)
                                    Dim enemy2 As New Enemy(2, 4, Math.Ceiling(Rnd() * 3) + 1)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                Case 3 '2 easy giclops, 1 medium ogre
                                    Dim enemy1 As New Enemy(1, 4, Math.Ceiling(Rnd() * 2) - 1)
                                    Dim enemy2 As New Enemy(2, 4, Math.Ceiling(Rnd() * 2) - 1)
                                    Dim enemy3 As New Enemy(3, 1, Math.Ceiling(Rnd() * 3) + 1)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                    localEnemyList.Add(enemy3)
                                Case 4 '2 hard ogres
                                    Dim enemy1 As New Enemy(1, 1, Math.Ceiling(Rnd() * 3) + 4)
                                    Dim enemy2 As New Enemy(2, 1, Math.Ceiling(Rnd() * 3) + 4)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                Case 5 '1 medium lich, 1 hard imp, 3 medium imps
                                    Dim enemy1 As New Enemy(1, 3, Math.Ceiling(Rnd() * 3) + 1)
                                    Dim enemy2 As New Enemy(2, 0, Math.Ceiling(Rnd() * 3) + 4)
                                    Dim enemy3 As New Enemy(3, 0, Math.Ceiling(Rnd() * 3) + 1)
                                    Dim enemy4 As New Enemy(4, 0, Math.Ceiling(Rnd() * 3) + 1)
                                    Dim enemy5 As New Enemy(5, 0, Math.Ceiling(Rnd() * 3) + 1)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                    localEnemyList.Add(enemy3)
                                    localEnemyList.Add(enemy4)
                                    localEnemyList.Add(enemy5)
                                Case 6 '1 hard lich, 1 hard imp, 1 medium imp
                                    Dim enemy1 As New Enemy(1, 3, Math.Ceiling(Rnd() * 3) + 4)
                                    Dim enemy2 As New Enemy(2, 0, Math.Ceiling(Rnd() * 3) + 4)
                                    Dim enemy3 As New Enemy(3, 0, Math.Ceiling(Rnd() * 3) + 1)
                                    localEnemyList.Add(enemy1)
                                    localEnemyList.Add(enemy2)
                                    localEnemyList.Add(enemy3)
                            End Select
                    End Select

                Case 3 'Denizens
                    Dim enemy As New Enemy(1, 6, 5) 'Thesis, the only boss at the moment
                    localEnemyList.Add(enemy)
                Case Else
                    Dim errorenemy As New Enemy(1, 0, 99) 'error imp, to avoid crashes
                    localEnemyList.Add(errorenemy)
            End Select

        End Sub

        'ADD: sub that orders players by turn order
        Public Sub OrderPlayersByAlacrity()
            'CODE THIS
        End Sub

        Public Sub BattleIntro()
            PlayMusic("battleintro", False)
            For x = 1 To 7
                Select Case x
                    Case 1
                        Console.ForegroundColor = ConsoleColor.DarkBlue
                    Case 2
                        Console.ForegroundColor = ConsoleColor.Blue
                    Case 3
                        Console.ForegroundColor = ConsoleColor.Cyan
                    Case 4
                        Console.ForegroundColor = ConsoleColor.Green
                    Case 5
                        Console.ForegroundColor = ConsoleColor.DarkYellow
                    Case 6
                        Console.ForegroundColor = ConsoleColor.Red
                    Case 7
                        Console.ForegroundColor = ConsoleColor.DarkRed
                End Select
                Threading.Thread.Sleep(200)
                ReadAndOutputTextFile("\data\asciibattleintro.txt", x)
            Next
            Console.ForegroundColor = ConsoleColor.White
            BlankLine()
            For x = 0 To enemyList.Count - 1
                If x > 0 Then
                    Console.Write(" & ")
                End If
                Console.Write(enemyList(x).name)
                If enemyList(x).hasTitle Then
                    Console.Write(", " & enemyList(x).title)
                End If
            Next
            Console.Write(" approach!")
            Beat()

            Select Case encounterType
                Case 1
                    battleMusic = "battle" & Math.Ceiling(Rnd() * 12)
                Case 2
                    battleMusic = "battle" & Math.Ceiling(Rnd() * 12) 'minibosses don't real
                Case Else
                    battleMusic = "denizen"
            End Select

        End Sub

        Public Sub PlayerInit()
            Console.Clear()
            BattleIntro()
            PlayMusic(battleMusic, True)
            playerarray(1).StartOfCombat()
            CheckVictory()
        End Sub

        Public Sub EnemyInit()
            enemyList.Clear()
            For x = 0 To localEnemyList.Count - 1
                enemyList.Add(localEnemyList(x))
            Next
        End Sub

        Public Sub PlayerTurns()
            Console.Clear()
            If playerarray(1).alive = True And Me.battleOver = False Then
                playerarray(1).TakeTurn()
            End If
            CheckVictory()
        End Sub

        Public Sub EnemyTurns()
            BlankLine()
            For x = 0 To enemyList.Count - 1
                If enemyList(x).alive = True And Me.battleOver = False Then
                    enemyList(x).TakeTurn()
                End If
                CheckVictory()
            Next
            Console.WriteLine("[Press Any Button to Continue]")
            Console.ReadKey()
            'Console.Clear()
        End Sub

        Public Sub CheckVictory()
            Dim victoryState As Integer = 2
            Dim anyEnemyAlive As Boolean = True
            '0 - No winner
            '1 - Player win
            '2 - Player loss

            playerarray(1).CheckAlive()
            If playerarray(1).alive Then
                victoryState = 0
            End If

            If victoryState = 0 Then
                anyEnemyAlive = False
                For x = 0 To enemyList.Count - 1
                    enemyList(x).CheckIfAlive()
                    If enemyList(x).alive Then
                        anyEnemyAlive = True
                    End If
                Next
            End If

            If anyEnemyAlive = False Then
                victoryState = 1
            End If

            Select Case victoryState
                Case 0
                    'Do Nothing
                Case 1
                    If Me.victoryHad = False Then
                        PlayerVictory()
                    End If
                Case 2
                    If Me.victoryHad = False Then
                        PlayerLoss()
                    End If
            End Select
        End Sub

        Public Sub PlayerVictory()
            Me.battleOver = True
            Me.victoryHad = True
            playerarray(1).EndOfCombat()
            PlayMusic("battlevictory", True)

            Console.Clear()
            Console.WriteLine("YOU WIN!")
        End Sub

        Public Sub PlayerLoss()
            Me.battleOver = True
            Me.victoryHad = True
            playerarray(1).EndOfCombat()
            PlayMusic("battleloss", True)

            Console.Clear()
            Console.WriteLine("YOU DIED!")
            BlankLine()
            Console.WriteLine("Press Enter to Continue")
            Console.ReadLine()

            'restart program, kinda
            Main()
        End Sub

    End Class

    Public Class NPCEvent
        Public NPCid As Integer
        Public NPCname As String
        Public fileName As String
        Public shopName As String
        Public trackName As String
        Public haveVisited As Boolean
        Public floor As Integer
        Public chatColour
        Public chatBG

        Public actionPool As New List(Of Integer)
        Public actionsForSale(5) As Integer
        Public actionPrices(5) As Integer
        Public priceVariance As Integer
        Public sideboard As New List(Of Integer)

        Public Sub New(id As Integer, floor As Integer)
            Me.NPCid = id
            Me.floor = floor
            Me.haveVisited = False

            Select Case Me.NPCid
                Case 0 'Vee
                    NPCname = "Vee"
                    trackName = "npcvee"
                    fileName = "vee"
                    shopName = "|Vee's VVares - Gr8 Prices for Gr8 Things|"
                    chatColour = ConsoleColor.DarkBlue
                    chatBG = ConsoleColor.Black
                    priceVariance = 50
                Case 1 'Laputis
                    NPCname = "Laputis"
                    trackName = "npclap"
                    fileName = "laputis"
                    shopName = "(| Trinkets and Knickknacks |) "
                    chatColour = ConsoleColor.Blue
                    chatBG = ConsoleColor.Black
                    priceVariance = 90
                Case 2 'Aki
                    NPCname = "Áki"
                    trackName = "npcaki"
                    fileName = "aki"
                    shopName = "put something here" 'PUT SOMETHING HERE AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
                    chatColour = ConsoleColor.Cyan
                    chatBG = ConsoleColor.Black
                    priceVariance = 10
                Case 3 '*:...
                    NPCname = "414345"
                    trackName = "npcace"
                    fileName = "ace"
                    shopName = "///ace.shopName///"
                    chatColour = ConsoleColor.Red
                    chatBG = ConsoleColor.Cyan
                    priceVariance = 100
                Case 4 'Specter
                    NPCname = "Specter"
                    trackName = "npcspecter"
                    fileName = "specter"
                    shopName = "(specter's stuff - will give you things for money)"
                    chatColour = ConsoleColor.Gray
                    chatBG = ConsoleColor.Black
                    priceVariance = 66
                Case 5 'El
                    NPCname = "Gift of El"
                    trackName = "npcpass"
                    fileName = "elpassant"
                    shopName = "[Battle Battallion's Bed & Breakfast]"
                    chatColour = ConsoleColor.Black
                    chatBG = ConsoleColor.White
                    priceVariance = 0
                Case 6 'Video
                    NPCname = "Video"
                    trackName = "npcvideo"
                    fileName = "video"
                    shopName = "<The Second Symphony - VCR Repair Shop>"
                    chatColour = ConsoleColor.Magenta
                    chatBG = ConsoleColor.Cyan
                    priceVariance = 30
                Case 7 'Guðrún
                    NPCname = "Guðrún"
                    trackName = "npcgudrun"
                    fileName = "gudrun"
                    shopName = "/Car(ry)ing Cargo/"
                    chatColour = ConsoleColor.Red
                    chatBG = ConsoleColor.Black
                    priceVariance = 20
            End Select

            InitActionPool()

            For x = 1 To 5
                actionPrices(x) = ReadTextFile("\data\actiongristcosts.txt", Me.actionsForSale(x) + 1)
                actionPrices(x) -= priceVariance / 2
                If actionPrices(x) <= 0 Then
                    actionPrices(x) = 1
                End If
                actionPrices(x) += Math.Ceiling(Rnd() * priceVariance)
            Next

        End Sub

        Public Sub InitActionPool()
            Dim counter As Integer = 1
            Dim tempString As String
            Dim tempInt As Integer
            Do
                tempString = ReadTextFile("\dialogue\" & fileName & "\actionpool.txt", counter)
                If tempString <> "" Then
                    tempInt = tempString
                    Me.actionPool.Add(tempInt)
                End If
                counter += 1
            Loop Until ReadTextFile("\dialogue\" & fileName & "\actionpool.txt", counter) = "-1"

            For x = 1 To 5
                counter = Math.Ceiling(Rnd() * Me.actionPool.Count) - 1
                Me.actionsForSale(x) = Me.actionPool(counter)
                Me.actionPool.RemoveAt(counter)
            Next
        End Sub

        Public Sub EnterShop()
            Dim playerInput As String
            Console.Clear()
            PlayMusic(Me.trackName, True)

            If Me.haveVisited = False Then
                Console.Write("You enter the shop")
                For x = 1 To 3
                    Console.Write(".")
                    Threading.Thread.Sleep(300)
                Next
                BlankLine()
                BlankLine()

                PlayMusic(Me.trackName, True)
                Say("introquotes", Math.Ceiling(Rnd() * 3))
                Me.haveVisited = True
            Else
                PlayMusic(Me.trackName, True)
                Say("idlequotes", Math.Ceiling(Rnd() * 3))
            End If

            BlankLine()
            Console.WriteLine(Me.shopName)
            BlankLine()
            ListGoods()
            BlankLine()
            Console.WriteLine("You have " & gristCache & " Grist.")
            If floor > 1 Then
                Console.WriteLine("[B]uy, [S]ideboard,")
                Console.WriteLine("[H]eal, or [L]eave?")
            Else
                Console.WriteLine("[B]uy, [H]eal,")
                Console.WriteLine("or [L]eave?")
            End If


            Dim inputAccepted As Boolean = False
            Do
                playerInput = Console.ReadKey(True).KeyChar
                Select Case LCase(playerInput)
                    Case "b"
                        inputAccepted = True
                        Console.Clear()
                        Buy()
                    Case "s"
                        If floor > 1 Then
                            inputAccepted = True
                            EditSideboard()
                        End If
                    Case "h"
                        inputAccepted = True
                        Heal()
                    Case "l"
                        inputAccepted = True
                        Console.Clear()
                        Say("leavequotes", Math.Ceiling(Rnd() * 3))
                        BlankLine()
                        Console.Write("You leave the shop")
                        For x = 1 To 3
                            Console.Write(".")
                            Threading.Thread.Sleep(300)
                        Next
                        BlankLine()
                        BlankLine()
                        Beat()
                        Console.Clear()
                End Select
            Loop While Not inputAccepted
        End Sub

        Public Sub Say(file As String, lineNo As Integer)
            Dim i1 As String = Console.ForegroundColor
            Dim i2 As String = Console.BackgroundColor
            Console.ForegroundColor = Me.chatColour
            Console.BackgroundColor = Me.chatBG

            Dim startLine As Integer = (lineNo * 5) - 4

            Dim sayString As String
            Dim typeSpeed As Integer
            For x = startLine To startLine + 4
                typeSpeed = 15
                sayString = ReadTextFile("\dialogue\" & Me.fileName & "\" & file & ".txt", x)

                If sayString <> "" Then
                    Console.Write(Me.NPCname & ": ")
                    For x2 = 0 To sayString.Length - 1
                        If sayString.Substring(x2, 1) = "." Then
                            typeSpeed = 100
                        End If

                        Console.Write(sayString.Substring(x2, 1))
                        Threading.Thread.Sleep(typeSpeed)
                        typeSpeed = 15
                    Next
                    Threading.Thread.Sleep(500)
                    Console.WriteLine("")
                End If


            Next

            Console.ForegroundColor = i1
            Console.BackgroundColor = i2
        End Sub

        Public Sub Buy()
            Dim playerInput As String

            Console.WriteLine("Which item?")
            BlankLine()
            ListGoods()
            BlankLine()
            Console.WriteLine("Or [G]o Back")
            BlankLine()
            playerInput = Console.ReadKey(True).KeyChar

            If playerInput = "1" Or playerInput = "2" Or playerInput = "3" Or playerInput = "4" Or playerInput = "5" Then
                If Me.actionPrices(playerInput) <> 0 And gristCache >= Me.actionPrices(playerInput) Then
                    playerarray(1).AddActionToArea(Me.actionsForSale(playerInput), -1)
                    gristCache -= Me.actionPrices(playerInput)
                    Me.actionPrices(playerInput) = 0
                    BlankLine()
                    Say("purchasequotes", Math.Ceiling(Rnd() * 3))
                    BlankLine()
                    Console.WriteLine("You bought [" & ReadTextFile("\data\actionnames.txt", Me.actionsForSale(playerInput) + 1) & "]!")
                    Beat()
                    EnterShop()
                ElseIf Me.actionPrices(playerInput) = 0 Then
                    BlankLine()
                    Console.WriteLine("That item is out of sale.")
                    Beat()
                    Buy()
                Else
                    BlankLine()
                    Console.WriteLine("You don't have enough grist to buy that.")
                    Beat()
                    Buy()
                End If
            Else
                BlankLine()
                Say("declinequotes", Math.Ceiling(Rnd() * 3))
                EnterShop()
            End If
        End Sub

        Public Sub ListGoods()
            Console.WriteLine("==============================")
            Threading.Thread.Sleep(100)
            For x = 1 To 5
                If Me.actionPrices(x) <> 0 Then
                    Console.WriteLine(x & " - [" & ReadTextFile("\data\actionnames.txt", Me.actionsForSale(x) + 1) & "] - (" & ReadTextFile("\data\actioncosts.txt", Me.actionsForSale(x) + 1) & ") - " & ReadTextFile("\data\actiondescriptions.txt", Me.actionsForSale(x) + 1))
                    Console.WriteLine("Cost: " & Me.actionPrices(x) & " Grist")
                Else
                    Console.WriteLine(x & " - SOLD OUT")
                End If

                If x <> 5 Then
                    BlankLine()
                End If
                Threading.Thread.Sleep(100)
            Next
            Console.WriteLine("==============================")
        End Sub

        Public Sub EditSideboard()
            Console.Clear()
            Console.WriteLine("Your Action Registry:")
            BlankLine()
            For x = 0 To playerarray(1).initialRegistry.Count - 1
                playerarray(1).WriteAction(x + 1, playerarray(1).initialRegistry(x))
                BlankLine()
            Next
            Console.WriteLine(Me.NPCname & "'s Sideboard:")
            BlankLine()
            For x = 0 To sideboard.Count - 1
                playerarray(1).WriteAction(x + 1, sideboard(x))
                BlankLine()
            Next
            Dim i As String = Console.ForegroundColor
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-")
            Console.WriteLine("Note: Each shopkeeper's sideboard is seperate; once you've")
            Console.WriteLine("left this floor, you can't retrieve anything you've left here.")
            Console.WriteLine("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-")
            Console.ForegroundColor = i
            BlankLine()
            Console.WriteLine("Move action [T]o or [F]rom this sideboard,")
            Console.WriteLine("or [R]eturn?")

            Dim playerInput As String
            Dim inputAccepted As Boolean = False
            Do
                playerInput = Console.ReadKey(True).KeyChar
                Select Case LCase(playerInput)
                    Case "t"
                        inputAccepted = True
                        moveToSideboard()
                    Case "f"
                        inputAccepted = True
                        moveFromSideboard()
                    Case "r"
                        inputAccepted = True
                        EnterShop()
                End Select
            Loop While Not inputAccepted
        End Sub

        Public Sub moveToSideboard()
            Console.Clear()
            Console.WriteLine("Which action to move?")
            BlankLine()
            For x = 0 To playerarray(1).initialRegistry.Count - 1
                playerarray(1).WriteAction(x + 1, playerarray(1).initialRegistry(x))
                BlankLine()
            Next

            Dim playerInput As Integer
            Dim inputAccepted As Boolean = False
            Dim actionToMove As Integer
            Do
                Console.Write("Input: ")
                playerInput = getCleanNumericalInput() - 1
                If playerInput > -1 And playerInput < playerarray(1).initialRegistry.Count Then
                    actionToMove = playerarray(1).initialRegistry(playerInput)
                    playerarray(1).initialRegistry.RemoveAt(playerInput)
                    Me.sideboard.Add(actionToMove)
                    inputAccepted = True
                Else
                    Console.WriteLine("Invalid choice.")
                    BlankLine()
                End If
            Loop While Not inputAccepted
            EditSideboard()
        End Sub
        Public Sub moveFromSideboard()
            Console.Clear()
            Console.WriteLine("Which action to move?")
            BlankLine()
            For x = 0 To Me.sideboard.Count - 1
                playerarray(1).WriteAction(x + 1, Me.sideboard(x))
                BlankLine()
            Next

            Dim playerInput As Integer
            Dim inputAccepted As Boolean = False
            Dim actionToMove As Integer
            Do
                Console.Write("Input: ")
                playerInput = getCleanNumericalInput() - 1
                If playerInput > -1 And playerInput < Me.sideboard.Count Then
                    actionToMove = Me.sideboard(playerInput)
                    Me.sideboard.RemoveAt(playerInput)
                    playerarray(1).initialRegistry.Add(actionToMove)
                    inputAccepted = True
                Else
                    Console.WriteLine("Invalid choice.")
                    BlankLine()
                End If
            Loop While Not inputAccepted
            EditSideboard()
        End Sub
        Public Sub Heal()
            Console.Clear()
            Dim missingHealth As Integer = playerarray(1).maxHealth - playerarray(1).currentHealth
            Dim fullHealCost As Integer = missingHealth * 10
            Dim canAffordFull As Boolean = False
            Dim canAffordPart As Boolean = False

            If fullHealCost <= gristCache Then
                canAffordFull = True
            End If
            If 100 <= gristCache Then
                canAffordPart = True
            End If
            If Not canAffordFull And Not canAffordPart Then
                Console.WriteLine("Not enough Grist to heal...")
            End If

            If playerarray(1).currentHealth <> playerarray(1).maxHealth And (canAffordFull Or canAffordPart) Then
                Console.WriteLine("Health: " & playerarray(1).currentHealth & "/" & playerarray(1).maxHealth & Space(30))
                Console.WriteLine("You have " & gristCache & " Grist.")
                BlankLine()
                If canAffordFull Then
                    Console.WriteLine("Heal [F]ully (" & missingHealth & " HP) for " & fullHealCost & " Grist,")
                End If
                If canAffordPart Then
                    Console.WriteLine("[P]artially (8 HP) for 100 Grist,")
                End If
                Console.WriteLine("or [R]eturn?")
                BlankLine()

                Dim playerInput As String
                Dim inputAccepted As Boolean = False
                Do
                    playerInput = Console.ReadKey(True).KeyChar
                    Select Case LCase(playerInput)
                        Case "f"
                            If canAffordFull Then
                                playerarray(1).currentHealth = playerarray(1).maxHealth
                                gristCache -= fullHealCost
                                Console.WriteLine("+" & missingHealth & " Health!")
                                Console.WriteLine("You're at full health!")
                                Beat()
                                inputAccepted = True
                                Heal()
                            End If
                        Case "p"
                            If canAffordPart Then
                                playerarray(1).currentHealth += 8
                                gristCache -= 100
                                Console.WriteLine("+8 Health!")
                                If playerarray(1).currentHealth > playerarray(1).maxHealth Then
                                    playerarray(1).currentHealth = playerarray(1).maxHealth
                                    Console.WriteLine("You're at full health!")
                                End If
                                Beat()
                                inputAccepted = True
                                Heal()
                            End If
                        Case "r"
                            inputAccepted = True
                            EnterShop()
                    End Select
                Loop While Not inputAccepted
            Else
                If playerarray(1).currentHealth = playerarray(1).maxHealth Then
                    Console.WriteLine("You're already at full health!")
                End If
                Beat()
                EnterShop()
            End If
        End Sub

    End Class

    Public Class DungeonFloor
        Public floorNo As Integer
        Public assignedPlayer As Integer
        Public dungeonGrid(127, 127) As Integer
        Public entranceUsed As Boolean = False
        Public storedXpos As Integer
        Public storedYpos As Integer
        Public trackName As String
        Public floorName As String
        Public floorFGColour As String
        Public floorBGColour As String

        Public enemyEncounters As New List(Of EnemyEncounter) 'ADD: generate EEs & NPCs
        Public NPCs As New List(Of NPCEvent) 'unused

        Public NPCshop As New NPCEvent(0, floorNo)

        Public numberOfenemyEncounters As Integer = 0
        Public numberOfNPCs As Integer = 0

        Public shouldEnemyMove As Boolean 'used to stop enemy movement after a non-movement input; idling still makes them move though

        Public Sub New(dungeonID As Integer, assignedPlayer As Integer)
            Me.floorNo = dungeonID

            'shopkeeper choice logic
            Dim NPCshopRandom As Integer = 8
            Do Until NPCHasSpawned(NPCshopRandom) = False
                If Me.floorNo < 2 Then
                    NPCshopRandom = 3
                    Do Until NPCshopRandom <> 3
                        NPCshopRandom = Math.Ceiling(Rnd() * 8) - 1
                    Loop
                Else
                    NPCshopRandom = Math.Ceiling(Rnd() * 8) - 1
                End If
            Loop
            NPCHasSpawned(NPCshopRandom) = True
            Dim ReplaceNPCShop As New NPCEvent(NPCshopRandom, floorNo)
            NPCshop = ReplaceNPCShop

            DungeonGenerateLayout()

            Dim trackNo As Integer

            Select Case floorNo
                Case 0
                    Me.floorName = "Prelude"
                    Me.floorFGColour = ConsoleColor.White
                    Me.floorBGColour = ConsoleColor.Black
                    trackNo = 0
                Case 1
                    Me.floorName = "Overgrowth"
                    Me.floorFGColour = ConsoleColor.DarkGreen
                    Me.floorBGColour = ConsoleColor.Black
                    trackNo = Math.Ceiling(Rnd() * 2)
                Case 2
                    Me.floorName = "Caverns"
                    Me.floorFGColour = ConsoleColor.DarkBlue
                    Me.floorBGColour = ConsoleColor.Cyan
                    trackNo = Math.Ceiling(Rnd() * 2) + 2
                Case 3
                    Me.floorName = "Outer Core"
                    Me.floorFGColour = ConsoleColor.Red
                    Me.floorBGColour = ConsoleColor.Yellow
                    trackNo = Math.Ceiling(Rnd() * 2) + 4
                Case 4
                    Me.floorName = "Inner Core"
                    Me.floorFGColour = ConsoleColor.Magenta
                    Me.floorBGColour = ConsoleColor.Gray
                    trackNo = Math.Ceiling(Rnd() * 2) + 6
                Case 5
                    Me.floorName = "Dearest Thesis"
                    Me.floorFGColour = ConsoleColor.Black
                    Me.floorBGColour = ConsoleColor.White
                    trackNo = Math.Ceiling(Rnd() * 2) + 8
            End Select

            Me.trackName = "dungeon" & trackNo
        End Sub

        Public Sub DungeonGenerateLayout()
            'legend
            '0 = wall
            '1 = empty room
            '2 = enemy encounter
            '3 = entrance
            '4 = exit
            '5 = shop

            '-1 = void, cannot be changed
            '6 = adjacent to room
            '7 = ^^, age 2
            '8 = so on

            'init
            For x = 0 To 127
                For y = 0 To 127
                    dungeonGrid(x, y) = 0
                Next
            Next
            dungeonGrid(64, 64) = 1

            DungeonVoidPerimeter()

            Dim dungeonSize As Integer
            Select Case floorNo
                Case 0
                    dungeonSize = 15
                Case 1
                    dungeonSize = 45
                Case 2
                    dungeonSize = 70
                Case 3
                    dungeonSize = 100
                Case 4
                    dungeonSize = 200
                Case 5
                    dungeonSize = 500
            End Select

            For i = 1 To dungeonSize
                DungeonAgePotentialRoomLocations()
                DungeonFindPotentialRoomLocations()
                DungeonPlaceRoom()
            Next

            'initialise object placement amounts, etc.
            Dim dungeonNoOfEnemyEncounters As Integer = Me.floorNo * 5
            Dim dungeonNoOfNPCs As Integer = Me.floorNo

            For i = 1 To dungeonNoOfEnemyEncounters
                DungeonPlaceObject(2)
            Next

            For i = 1 To 10
                DungeonAgePotentialRoomLocations()
            Next

            'Devoiding
            For x = 0 To 127
                For y = 0 To 127
                    If dungeonGrid(x, y) = -1 Then
                        dungeonGrid(x, y) = 0
                    End If
                Next
            Next

            'any unique layout generation for each floor goes here
            Select Case floorNo
                Case 0
                    DungeonSpiralGeneration(64, 64, Math.Ceiling(Rnd() * 3) + 5)
                Case 1
                    DungeonSmoothLayout(3)
                Case 2
                    'nothing
                Case 3
                    'nothing
                Case 4
                    DungeonSmoothLayout(4)
                    DungeonSpiralGeneration(64, 64, Math.Ceiling(Rnd() * 10))
                Case 5
                    DungeonSmoothLayout(4)
                    DungeonSpiralGeneration(64, 64, Math.Ceiling(Rnd() * 5) + 15)
            End Select

            Dim distFromOrigin(127, 127) As Double
            For x = 0 To 127
                For y = 0 To 127
                    If dungeonGrid(x, y) = 1 Then
                        distFromOrigin(x, y) = DistanceBetweenTwoRooms(64, 64, x, y)
                    End If
                Next
            Next

            Dim furthestRoomX As Integer
            Dim furthestRoomY As Integer
            Dim furthestDist As Double = 0
            For x = 0 To 127
                For y = 0 To 127
                    If distFromOrigin(x, y) > furthestDist And dungeonGrid(x, y) = 1 Then
                        furthestDist = distFromOrigin(x, y)
                        furthestRoomX = x
                        furthestRoomY = y
                    End If
                Next
            Next

            dungeonGrid(furthestRoomX, furthestRoomY) = 3

            Dim distFromEntrance(127, 127) As Double
            For x = 0 To 127
                For y = 0 To 127
                    If dungeonGrid(x, y) = 1 Then
                        distFromEntrance(x, y) = DistanceBetweenTwoRooms(DungeonFindRoom(True, 3), DungeonFindRoom(False, 3), x, y)
                    End If
                Next
            Next

            furthestDist = 0

            For x = 0 To 127
                For y = 0 To 127
                    If distFromEntrance(x, y) > furthestDist And dungeonGrid(x, y) = 1 Then
                        furthestDist = distFromEntrance(x, y)
                        furthestRoomX = x
                        furthestRoomY = y
                    End If
                Next
            Next

            dungeonGrid(furthestRoomX, furthestRoomY) = 4

            DungeonPlaceObject(5)

        End Sub

        Public Sub DungeonVoidPerimeter()
            For x = 0 To 127
                For y = 0 To 127
                    If x = 0 Or x = 127 Or y = 0 Or y = 127 Or x = 1 Or x = 126 Or y = 1 Or y = 126 Then
                        dungeonGrid(x, y) = -1
                    End If
                Next
            Next
        End Sub

        Public Sub DungeonAgePotentialRoomLocations()
            For x = 0 To 127
                For y = 0 To 127
                    If dungeonGrid(x, y) > 5 Then
                        dungeonGrid(x, y) += 1
                    End If
                    If dungeonGrid(x, y) = 10 Then
                        dungeonGrid(x, y) = -1
                    End If
                Next
            Next
        End Sub

        Public Sub DungeonFindPotentialRoomLocations()
            For x = 0 To 127
                For y = 0 To 127
                    If dungeonGrid(x, y) = 1 Then
                        If dungeonGrid(x + 1, y) = 0 Then
                            dungeonGrid(x + 1, y) = 6
                        End If
                        If dungeonGrid(x - 1, y) = 0 Then
                            dungeonGrid(x - 1, y) = 6
                        End If
                        If dungeonGrid(x, y + 1) = 0 Then
                            dungeonGrid(x, y + 1) = 6
                        End If
                        If dungeonGrid(x, y - 1) = 0 Then
                            dungeonGrid(x, y - 1) = 6
                        End If
                    End If
                Next
            Next
        End Sub

        Public Sub DungeonPlaceRoom()
            Dim roomPlaced As Boolean = False
            Dim infiniteLoopPreventionCounter As Integer = 0
            Do
                Dim randx As Integer = Math.Ceiling(Rnd() * 127)
                Dim randy As Integer = Math.Ceiling(Rnd() * 127)
                If dungeonGrid(randx, randy) > 5 Then
                    dungeonGrid(randx, randy) = 1
                    roomPlaced = True
                End If

                infiniteLoopPreventionCounter += 1
            Loop Until roomPlaced = True Or infiniteLoopPreventionCounter > 10000

            If infiniteLoopPreventionCounter > 10000 Then
                For x = 0 To 14
                    For y = 0 To 14
                        If dungeonGrid(x, y) = -1 Then
                            dungeonGrid(x, y) = 0
                        End If
                    Next
                Next
                DungeonVoidPerimeter()
            End If
        End Sub

        Public Sub DungeonPlaceObject(objectToPlace As Integer)
            Dim objectPlaced As Boolean = False
            Dim infiniteLoopPreventionCounter As Integer = 0
            Do
                Dim randx As Integer = Math.Ceiling(Rnd() * 127)
                Dim randy As Integer = Math.Ceiling(Rnd() * 127)
                If dungeonGrid(randx, randy) = 1 Then
                    dungeonGrid(randx, randy) = objectToPlace
                    objectPlaced = True
                End If
                infiniteLoopPreventionCounter += 1
            Loop Until objectPlaced = True Or infiniteLoopPreventionCounter > 10000

            If objectPlaced = False Then 'important object fails to place, add rooms and try again
                DungeonFindPotentialRoomLocations()
                DungeonAgePotentialRoomLocations()
                DungeonPlaceRoom()

                DungeonPlaceObject(objectToPlace)
            End If
        End Sub

        Public Sub GenerateEEsandNPCs()
            For y = 0 To 127
                For x = 0 To 127
                    If dungeonGrid(x, y) = 2 Then 'Generate enemy encounter
                        Dim newEE As New EnemyEncounter(1, 0)
                        enemyEncounters.Add(newEE)
                        numberOfenemyEncounters += 1
                    ElseIf dungeonGrid(x, y) = 3 Then 'Generate NPC
                        Dim newNPC As New NPCEvent(-1, 0)
                        NPCs.Add(newNPC)
                        numberOfNPCs += 1
                    ElseIf dungeonGrid(x, y) = 5 Then 'Generate miniboss
                        Dim newEE As New EnemyEncounter(2, 0)
                        enemyEncounters.Add(newEE)
                        numberOfenemyEncounters += 1
                    End If
                Next
            Next
        End Sub

        Public Sub DungeonSmoothLayout(adjRoomsReq As Integer)
            Dim adjRoomCounter As Integer
            Dim roomsToAdd(126, 126) As Integer
            For x = 1 To 126
                For y = 1 To 126
                    If dungeonGrid(x, y) = 0 Then
                        adjRoomCounter = 0

                        If dungeonGrid(x + 1, y) <> 0 Then
                            adjRoomCounter += 1
                        End If
                        If dungeonGrid(x - 1, y) <> 0 Then
                            adjRoomCounter += 1
                        End If
                        If dungeonGrid(x, y + 1) <> 0 Then
                            adjRoomCounter += 1
                        End If
                        If dungeonGrid(x, y - 1) <> 0 Then
                            adjRoomCounter += 1
                        End If

                        If adjRoomCounter >= adjRoomsReq Then
                            roomsToAdd(x, y) = 1
                        End If
                    End If
                Next
            Next

            For x = 1 To 126
                For y = 1 To 126
                    If roomsToAdd(x, y) = 1 Then
                        dungeonGrid(x, y) = 1
                    End If
                Next
            Next
        End Sub

        Public Sub DungeonSpiralGeneration(startX As Integer, startY As Integer, spiralLength As Integer)
            Dim turtleX As Integer = startX
            Dim turtleY As Integer = startY
            Dim moveLength As Integer = 2
            Dim moveDir As Integer = 1
            Dim lengthChangeCounter As Integer = 0

            For i = 1 To spiralLength
                For i2 = 1 To moveLength
                    dungeonGrid(turtleX, turtleY) = 1
                    Select Case moveDir
                        Case 1
                            turtleX += 1
                        Case 2
                            turtleY -= 1
                        Case 3
                            turtleX -= 1
                        Case 4
                            turtleY += 1
                    End Select
                Next
                If lengthChangeCounter = 0 Then
                    lengthChangeCounter = 1
                Else
                    lengthChangeCounter = 0
                    moveLength += 2
                End If
                moveDir += 1
                If moveDir = 5 Then
                    moveDir = 1
                End If
            Next
        End Sub

        Public Function DungeonFindRoom(returnx As Boolean, roomToFind As Integer)
            For y = 0 To 127
                For x = 0 To 127
                    If dungeonGrid(x, y) = roomToFind Then
                        If returnx Then
                            Return x
                        Else
                            Return y
                        End If
                    End If
                Next
            Next
        End Function

        Public Function DungeonFindPotentialExits(exitNo As Integer, returnx As Boolean)
            Dim potExits(10, 1) As Integer
            Dim counter As Integer = 1
            For y = 0 To 127
                For x = 0 To 127
                    If dungeonGrid(x, y) = 11 Then
                        potExits(counter, 0) = x
                        potExits(counter, 1) = y
                        counter += 1
                    End If
                Next
            Next
            If returnx Then
                Return potExits(exitNo, 0)
            Else
                Return potExits(exitNo, 1)
            End If
        End Function

        Public Function DistanceBetweenTwoRooms(room1x As Integer, room1y As Integer, room2x As Integer, room2y As Integer)
            Return Math.Sqrt(Math.Abs(room1x - room2x) + Math.Abs(room1y - room2y))
        End Function

        Public Sub DungeonDrawMap()
            For y = 0 To 127
                For x = 0 To 127
                    Select Case dungeonGrid(x, y)
                        Case -1
                            Console.Write("█")
                        Case 0
                            Console.Write("█")
                        Case 1
                            Console.Write("░")
                        Case 2
                            Console.Write("e")
                        Case 3
                            Console.Write("±")
                        Case 4
                            Console.Write("0")
                        Case 5
                            Console.Write("E")
                        Case Else
                            Console.Write("?")
                    End Select
                Next
                Console.WriteLine("")
            Next
        End Sub

        Public Sub DungeonDrawPlayerMap()
            For y = 0 To 127
                For x = 0 To 127
                    If x = dungeonXPos And y = dungeonYPos Then
                        Console.Write("#")
                    Else
                        Select Case dungeonGrid(x, y)
                            Case -1
                                Console.Write("█")
                            Case 0
                                Console.Write("█")
                            Case 1
                                Console.Write("░")
                            Case 2
                                Console.Write("░")
                            Case 3
                                Console.Write("0")
                            Case 4
                                Console.Write("±")
                            Case 5
                                Console.Write("░")
                            Case Else
                                Console.Write("?")
                        End Select
                    End If
                Next
                Console.WriteLine("")
            Next
        End Sub

        Public Sub DungeonDraw3x3AreaOLD(centerx As Integer, centery As Integer)
            Dim SectionOfDungeonMap(2, 2) As Integer

            For y1 = 0 To 2
                For x1 = 0 To 2
                    SectionOfDungeonMap(x1, y1) = dungeonGrid(centerx - 1 + x1, centery - 1 + y1)
                Next
            Next

            Dim midString As String
            Console.WriteLine("░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░")
            Console.WriteLine("░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░")
            Console.WriteLine("░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒░")
            For y = 0 To 2
                Console.Write("░▒▓")
                For x = 0 To 2
                    Select Case SectionOfDungeonMap(x, y)
                        Case -1
                            Console.Write("█████████")
                        Case 0
                            Console.Write("█████████")
                        Case Else
                            Console.Write("┌───────┐")
                    End Select
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
                Console.Write("░▒▓")
                For x = 0 To 2
                    Select Case SectionOfDungeonMap(x, y)
                        Case -1
                            Console.Write("█████████")
                        Case 0
                            Console.Write("█████████")
                        Case Else
                            Console.Write("│       │")
                    End Select
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
                Console.Write("░▒▓")
                For x = 0 To 2
                    Select Case SectionOfDungeonMap(x, y)
                        Case 0
                            midString = "█████████"
                        Case 1
                            midString = "│       │"
                        Case 2
                            midString = "│ ENEMY │"
                        Case 3
                            midString = "│  NPC  │"
                        Case 4
                            midString = "│ENTRNCE│"
                        Case 5
                            midString = "│MINBOSS│"
                        Case -1
                            midString = "█████████"
                        Case Else
                            midString = "│ ERROR │"
                    End Select

                    Console.Write(midString)
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
                Console.Write("░▒▓")
                For x = 0 To 2
                    Select Case SectionOfDungeonMap(x, y)
                        Case -1
                            Console.Write("█████████")
                        Case 0
                            Console.Write("█████████")
                        Case Else
                            Console.Write("│       │")
                    End Select
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
                Console.Write("░▒▓")
                For x = 0 To 2
                    Select Case SectionOfDungeonMap(x, y)
                        Case -1
                            Console.Write("█████████")
                        Case 0
                            Console.Write("█████████")
                        Case Else
                            Console.Write("└───────┘")
                    End Select
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
            Next
            Console.WriteLine("░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒░")
            Console.WriteLine("░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░")
            Console.WriteLine("░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░")
        End Sub

        Public Sub DungeonDraw5x5Area(centerx As Integer, centery As Integer)
            Dim SectionOfDungeonMap(4, 4) As Integer

            If centerx <= 1 Then
                centerx = 2
            ElseIf centerx >= 126 Then
                centerx = 125
            End If

            If centery <= 1 Then
                centery = 2
            ElseIf centery >= 126 Then
                centery = 125
            End If

            For y1 = 0 To 4
                For x1 = 0 To 4
                    SectionOfDungeonMap(x1, y1) = dungeonGrid(centerx - 2 + x1, centery - 2 + y1)
                Next
            Next

            Console.WriteLine("Floor " & floorNo & " - " & floorName)
            BlankLine()

            Dim i1 As String = Console.ForegroundColor
            Dim i2 As String = Console.BackgroundColor
            Console.ForegroundColor = Me.floorFGColour
            Console.BackgroundColor = Me.floorBGColour

            Dim midString As String
            Console.WriteLine("░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░")
            Console.WriteLine("░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░")
            Console.WriteLine("░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒░")
            For y = 0 To 4
                Console.Write("░▒▓")
                For x = 0 To 4
                    Select Case SectionOfDungeonMap(x, y)
                        Case -1
                            Console.Write("█████████")
                        Case 0
                            Console.Write("█████████")
                        Case Else
                            Console.Write("┌───────┐")
                    End Select
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
                Console.Write("░▒▓")
                For x = 0 To 4
                    Select Case SectionOfDungeonMap(x, y)
                        Case -1
                            Console.Write("█████████")
                        Case 0
                            Console.Write("█████████")
                        Case Else
                            Console.Write("│       │")
                    End Select
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
                Console.Write("░▒▓")
                For x = 0 To 4
                    If x = 2 And y = 2 Then
                        midString = "│  YOU  │"
                    Else
                        Select Case SectionOfDungeonMap(x, y)
                            Case 0
                                midString = "█████████"
                            Case 1
                                midString = "│       │"
                            Case 2
                                midString = "│ ENEMY │"
                            Case 3
                                midString = "│ENTRNCE│"
                            Case 4
                                midString = "│  EXIT │"
                            Case 5
                                midString = "│  SHOP │"
                            Case -1
                                midString = "█████████"
                            Case Else
                                midString = "│ ERROR │"
                        End Select
                    End If

                    Console.Write(midString)
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
                Console.Write("░▒▓")
                For x = 0 To 4
                    Select Case SectionOfDungeonMap(x, y)
                        Case -1
                            Console.Write("█████████")
                        Case 0
                            Console.Write("█████████")
                        Case Else
                            Console.Write("│       │")
                    End Select
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
                Console.Write("░▒▓")
                For x = 0 To 4
                    Select Case SectionOfDungeonMap(x, y)
                        Case -1
                            Console.Write("█████████")
                        Case 0
                            Console.Write("█████████")
                        Case Else
                            Console.Write("└───────┘")
                    End Select
                Next
                Console.Write("▓▒░")
                Console.WriteLine()
            Next
            Console.WriteLine("░▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒░")
            Console.WriteLine("░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░")
            Console.WriteLine("░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░")

            Console.ForegroundColor = i1
            Console.BackgroundColor = i2
        End Sub

        Public Sub MovePlayer(xMovement As Integer, yMovement As Integer)
            Dim prevX As Integer = dungeonXPos
            Dim prevY As Integer = dungeonYPos

            dungeonXPos += xMovement
            dungeonYPos += yMovement

            If dungeonGrid(dungeonXPos, dungeonYPos) = -1 Or dungeonGrid(dungeonXPos, dungeonYPos) = 0 Then
                dungeonXPos = prevX
                dungeonYPos = prevY
            End If

            If dungeonXPos > 125 Then
                dungeonXPos = 1
            ElseIf dungeonXPos < 1 Then
                dungeonXPos = 125
            End If
            If dungeonYPos > 125 Then
                dungeonYPos = 1
            ElseIf dungeonYPos < 1 Then
                dungeonYPos = 125
            End If

        End Sub

        Public Sub ExploreDungeon()

            If entranceUsed = False Then
                dungeonXPos = DungeonFindRoom(True, 3)
                dungeonYPos = DungeonFindRoom(False, 3)
                entranceUsed = True
            Else
                dungeonXPos = storedXpos
                dungeonYPos = storedYpos
            End If

            PlayMusic(trackName, True)
            Dim hasCollided As Boolean = False
            Console.Clear()

            Do
                shouldEnemyMove = True
                Console.SetCursorPosition(0, 0)
                DungeonDraw5x5Area(dungeonXPos, dungeonYPos)
                BlankLine()
                If displayPlayerCoords Then
                    Console.WriteLine(dungeonXPos & Space(1) & dungeonYPos)
                    BlankLine()
                End If
                Console.WriteLine("WASD - Move")
                BlankLine()
                Console.WriteLine("I - Character Sheet")
                Console.WriteLine("M - Open Map")
                BlankLine()
                Console.WriteLine("1 - Save")
                Console.WriteLine("2 - Quit")
                BlankLine()
                Select Case LCase(Console.ReadKey(True).KeyChar)
                    Case "a"
                        MovePlayer(-1, 0)
                    Case "s"
                        MovePlayer(0, 1)
                    Case "d"
                        MovePlayer(1, 0)
                    Case "w"
                        MovePlayer(0, -1)
                    Case "m"
                        Console.Clear()
                        DungeonDrawPlayerMap()
                        BlankLine()
                        Console.WriteLine("Legend:")
                        Console.WriteLine("# - You")
                        Console.WriteLine("0 - Entrance")
                        Console.WriteLine("± - Exit")
                        BlankLine()
                        Console.WriteLine("[Press Any Button to Close]")
                        Console.ReadKey()
                        Console.Clear()
                        shouldEnemyMove = False
                    Case "i"
                        Console.Clear()
                        playerarray(1).WriteShortPlayerInfo()
                        BlankLine()

                        Console.WriteLine("Your Action Registry:")
                        BlankLine()
                        For x = 0 To playerarray(1).initialRegistry.Count - 1
                            playerarray(1).WriteAction(x + 1, playerarray(1).initialRegistry(x))
                            BlankLine()
                        Next

                        Console.WriteLine("[Press Any Button to Close]")
                        Console.ReadKey()
                        Console.Clear()
                        shouldEnemyMove = False
                    Case "1"
                        SaveRun("\saves\testsave.txt")
                        shouldEnemyMove = False
                    Case "2"
                        MainMenu()
                    Case "z"
                        dungeonXPos = DungeonFindRoom(True, 4)
                        dungeonYPos = DungeonFindRoom(False, 4)
                End Select

                'enemies move
                If shouldEnemyMove Then
                    EnemyMove()
                End If

                'check for enemy/exit collisions

                If dungeonGrid(dungeonXPos, dungeonYPos) = 2 Or dungeonGrid(dungeonXPos, dungeonYPos) = 4 Or dungeonGrid(dungeonXPos, dungeonYPos) = 5 Then
                    hasCollided = True
                End If
            Loop Until hasCollided

            Console.Clear()
            storedXpos = dungeonXPos
            storedYpos = dungeonYPos

            Select Case dungeonGrid(dungeonXPos, dungeonYPos)
                Case 2
                    CombatLoop(1, floorNo)
                    dungeonGrid(dungeonXPos, dungeonYPos) = 1
                    ExploreDungeon()
                Case 4
                    Console.WriteLine("Descend?")
                    Console.WriteLine("[Y]es or [N]o?")
                    BlankLine()
                    Select Case LCase(Console.ReadKey(True).KeyChar)
                        Case "y"
                            'down down
                        Case Else
                            ExploreDungeon()
                    End Select
                Case 5
                    Console.WriteLine("Enter Shop?")
                    Console.WriteLine("[Y]es or [N]o?")
                    BlankLine()
                    Select Case LCase(Console.ReadKey(True).KeyChar)
                        Case "y"
                            Me.NPCshop.EnterShop()
                            ExploreDungeon()
                        Case Else
                            ExploreDungeon()
                    End Select
                Case Else
                    ExploreDungeon()
            End Select
        End Sub

        Public Sub EnemyMove()
            Dim EnemyLocationX As New List(Of Integer)
            Dim EnemyLocationY As New List(Of Integer)
            For x = 0 To 127
                For y = 0 To 127
                    If dungeonGrid(x, y) = 2 Then
                        EnemyLocationX.Add(x)
                        EnemyLocationY.Add(y)
                    End If
                Next
            Next

            If EnemyLocationX.Count > 0 Then

                Dim enemyMap(127, 127) As Integer 'simplify the regular map array unto just empty rooms (1) and everything else (0)
                For x = 0 To 127
                    For y = 0 To 127
                        If dungeonGrid(x, y) = 1 Then
                            enemyMap(x, y) = 1
                        Else
                            enemyMap(x, y) = 0
                        End If
                    Next
                Next

                For i = 0 To EnemyLocationX.Count - 1
                    Dim targetX As Integer
                    Dim targetY As Integer

                    Dim targetType As Integer
                    Dim moveChance As Integer
                    If DistanceBetweenTwoRooms(EnemyLocationX(i), EnemyLocationY(i), dungeonXPos, dungeonYPos) <= 1 Then
                        targetType = 0
                        moveChance = 2
                    ElseIf DistanceBetweenTwoRooms(EnemyLocationX(i), EnemyLocationY(i), dungeonXPos, dungeonYPos) <= 4 Then
                        targetType = 0
                        moveChance = 4
                    Else
                        targetType = 1
                        moveChance = 10
                    End If


                    If Math.Ceiling(Rnd() * moveChance) = 1 Then
                        'Choosing the Target
                        If targetType = 0 Then 'if player is within 5 units, they're the target location
                            targetX = dungeonXPos
                            targetY = dungeonYPos
                        Else 'else, a random room within a 5 x 5 cube around them is the target location
                            Do
                                targetX = (EnemyLocationX(i) - 3) + Math.Ceiling(Rnd() * 5)
                                targetY = (EnemyLocationY(i) - 3) + Math.Ceiling(Rnd() * 5)
                            Loop Until enemyMap(targetX, targetY) = 1
                        End If

                        Dim closestSurroundingDistance As Double = 10000
                        Dim closestSurroundingX As Integer = 1
                        Dim closestSurroundingY As Integer = 1
                        For x = 0 To 2
                            For y = 0 To 2
                                If closestSurroundingDistance > DistanceBetweenTwoRooms((EnemyLocationX(i) - 1) + x, (EnemyLocationY(i) - 1) + y, targetX, targetY) And enemyMap((EnemyLocationX(i) - 1) + x, (EnemyLocationY(i) - 1) + y) = 1 Then
                                    closestSurroundingDistance = DistanceBetweenTwoRooms((EnemyLocationX(i) - 1) + x, (EnemyLocationY(i) - 1) + y, targetX, targetY)
                                    closestSurroundingX = x
                                    closestSurroundingY = y
                                End If
                            Next
                        Next

                        closestSurroundingX -= 1
                        closestSurroundingY -= 1

                        dungeonGrid(EnemyLocationX(i), EnemyLocationY(i)) = 1
                        dungeonGrid(EnemyLocationX(i) + closestSurroundingX, EnemyLocationY(i) + closestSurroundingY) = 2

                        If displayEnemyMovements Then
                            Console.WriteLine("enemy " & i & " moved to " & EnemyLocationX(i) + closestSurroundingX & "," & EnemyLocationY(i) + closestSurroundingY)
                            Console.ReadLine()
                        End If
                    End If
                Next
            End If
        End Sub

        Public Sub Unstuck()
            Dim playerStuck As Boolean = True
            If dungeonGrid(dungeonXPos, dungeonYPos) <> 0 Then
                If dungeonGrid(dungeonXPos + 1, dungeonYPos) = 1 Then
                    playerStuck = False
                ElseIf dungeonGrid(dungeonXPos, dungeonYPos + 1) = 1 Then
                    playerStuck = False
                ElseIf dungeonGrid(dungeonXPos - 1, dungeonYPos) = 1 Then
                    playerStuck = False
                ElseIf dungeonGrid(dungeonXPos, dungeonYPos - 1) = 1 Then
                    playerStuck = False
                End If
            End If

            If playerStuck Then
                dungeonXPos = DungeonFindRoom(True, 3)
                dungeonYPos = DungeonFindRoom(False, 3)
            End If
        End Sub

    End Class

    Sub Main()
        'Initialisation
        FillPlayerArray() 'Fills up the player arrays with randomly generated players
        ResetPlayerArray() 'Sets all of the player array slots to "Null", so they are ignored
        Randomize()

        Do
            Console.Clear()
            MainMenu()
            End
        Loop
    End Sub

    'Menus
    Public Sub WriteLogo()
        For x = 1 To 6
            Threading.Thread.Sleep(200)

            Select Case x
                Case 1
                    Console.ForegroundColor = ConsoleColor.Blue
                Case 2
                    Console.ForegroundColor = ConsoleColor.Cyan
                Case 3
                    Console.ForegroundColor = ConsoleColor.Green
                Case 4
                    Console.ForegroundColor = ConsoleColor.DarkYellow
                Case 5
                    Console.ForegroundColor = ConsoleColor.Red
                Case 6
                    Console.ForegroundColor = ConsoleColor.DarkRed
            End Select

            Console.WriteLine(ReadTextFile("\data\asciititle.txt", x))
        Next
        Console.ForegroundColor = ConsoleColor.White
        Console.BackgroundColor = ConsoleColor.Black
    End Sub

    Public Sub MainMenu()

        Console.Write("Generating Content")
        theDungeon.Clear()
        For x = 0 To 7
            NPCHasSpawned(x) = False
        Next
        NPCHasSpawned(8) = True 'this value used during shop generation to allow loop to start
        For x = 0 To 5
            Dim newDungeonFloor As New DungeonFloor(x, 1)
            theDungeon.Add(newDungeonFloor)
            Console.Write(".")
        Next
        GenerateNewPlayer()
        Console.Clear()
        If menuMusicOnLaunch Then
            PlayMusic("mainmenu", True)
            menuMusicOnLaunch = False
        End If
        Console.CursorVisible = False

        Dim playerinput As String = ""

        Console.Clear()
        WriteLogo()
        Console.WriteLine("==> Version 1.0")
        Console.WriteLine("a game by Panfex, based on Andrew Hussie's Homestuck")
        BlankLine()
        Console.WriteLine("1 - Start New Run")
        Console.WriteLine("2 - Load Previous Run")
        Console.WriteLine("3 - Options & Extras")
        Console.WriteLine("4 - Credits")
        BlankLine()
        Console.WriteLine("0 - Exit")
        BlankLine()
        Do
            playerinput = Console.ReadKey(True).KeyChar
        Loop Until ReturnNumbersOnly(playerinput) <> ""

        Select Case playerinput
            Case 1
                GameStructure(0)
                MainMenu()
            Case 2
                LoadRun("\saves\testsave.txt")

                Dim startingFloor As Integer
                For x = 0 To 5
                    If theDungeon(x).entranceUsed = True Then
                        startingFloor = x
                    End If
                Next

                GameStructure(startingFloor)
                MainMenu()
            Case 3 'temp until options implemented
                Options()
                MainMenu()
            Case 4
                Console.Clear()
                Credits()
                Console.ReadLine()
                MainMenu()
            Case 9
                Console.Clear()
                DebugMenu()
                MainMenu()
            Case 0
                Console.Clear()
                Console.WriteLine("Thanks for playing!")
                Beat()
                End
            Case Else
                Console.Clear()
                MainMenu()
        End Select
    End Sub

    Public Sub GameStructure(startingFloor As Integer)
        Console.Clear()
        Console.WriteLine("You've entered the Denizen's Dungeon...")
        Console.WriteLine("Your objective is to reach the 5th floor of the dungeon and slay its Denizen.")
        BlankLine()
        Console.WriteLine("Navigate to each floor's exit to descend to the next one,")
        Console.WriteLine("try to find shopkeepers along the way who can assist you")
        Console.WriteLine("and beware the great many underlings who'll try to stop you along the way.")
        BlankLine()
        Console.WriteLine("Good luck down there, " & playerarray(1).firstName & " " & playerarray(1).lastName & ", " & ClassIntToString(playerarray(1).classTitle) & " Of " & AspectIntToString(playerarray(1).aspectTitle) & "...")
        BlankLine()
        Console.WriteLine("[PRESS ANY KEY TO START]")
        Console.ReadKey()

        For x = startingFloor To 5
            theDungeon(x).ExploreDungeon()
        Next
        CombatLoop(3, 5)

        PlayMusic("battlevictory", True)
        Console.Clear()
        Console.WriteLine("You have defeated the Denizen's Dungeon! Congratulations!")
        BlankLine()
        Console.WriteLine("[PRESS ANY KEY TO CONTINUE]")
        Console.ReadKey()

        Console.Clear()
        Credits()
        Console.ReadKey()
    End Sub

    Public Sub CombatLoop(encounterType As Integer, dungeonfloor As Integer)
        Dim Encounter As New EnemyEncounter(encounterType, dungeonfloor)
        Encounter.EnemyInit()
        Encounter.PlayerInit()

        Do
            Encounter.PlayerTurns()
            Encounter.EnemyTurns()
        Loop Until Encounter.battleOver = True
    End Sub
    Public Sub Options()
        Console.Clear()
        Console.Write("Options")
        BlankLine()
        Console.Write("[M]ute Music? - ")
        If isMusicMuted Then
            Console.WriteLine("YES")
        Else
            Console.WriteLine("NO")
        End If
        BlankLine()
        Console.WriteLine("[R]eturn to Menu")

        Dim playerInput As String
        Dim inputAccepted As Boolean = False
        Do
            playerInput = Console.ReadKey(True).KeyChar
            Select Case LCase(playerInput)
                Case "m"
                    If isMusicMuted Then
                        isMusicMuted = False
                        PlayMusic("dungeon0", True)
                    Else
                        isMusicMuted = True
                        My.Computer.Audio.Stop()
                        currentlyPlayingTrack = ""
                    End If
                    Options()
                    inputAccepted = True
                Case "r"
                    inputAccepted = True
            End Select
        Loop While Not inputAccepted
        Console.Clear()
    End Sub

    Public Sub Credits()
        Dim counter As Integer = 1
        Dim creditsString As String = ""
        PlayMusic("credits", True)

        Do
            creditsString = ReadTextFile("\data\credits.txt", counter)
            Threading.Thread.Sleep(500)
            Console.WriteLine(creditsString)
            counter += 1
        Loop Until creditsString = "-/CREDITS-"
    End Sub
    Public Sub DebugMenu()
        'For x = 1 To 120
        'playerarray(1).WriteAction(x, x)
        'BlankLine()
        'Next

        Dim endLoop As Boolean = False
        Dim currentTrack As Integer = 0
        Do
            Console.Write("Sound Test: ")
            'SoundTest(getCleanNumericalInput(Console.ReadLine()))
            Select Case LCase(SoundTest(currentTrack))
                Case "a"
                    currentTrack -= 1
                    If currentTrack < 0 Then
                        currentTrack = 37
                    End If
                Case "d"
                    currentTrack += 1
                    If currentTrack > 37 Then
                        currentTrack = 0
                    End If
                Case "0"
                    endLoop = True
            End Select
        Loop Until endLoop

    End Sub

    'Generation Stuffs

    Public Sub GenerateNewPlayer()
        Dim randomlygeneratedhuman As New Player
        randomlygeneratedhuman.playerId = 1
        randomlygeneratedhuman.HumanGen()
        randomlygeneratedhuman.RoundTotalStats(60)
        randomlygeneratedhuman.GenerateStartingRegistry()
        randomlygeneratedhuman.SortInitReg()
        playerarray(1) = randomlygeneratedhuman
    End Sub

    'Saving/Loading

    Public Sub SaveRun(filedir)
        Console.Clear()
        Console.Write("Saving")
        FileOpen(2, CurDir() & filedir, OpenMode.Output)

        For x = 0 To 5
            WriteLine(2, theDungeon(x).floorNo)
            For y1 = 0 To 127
                For x1 = 0 To 127
                    If x1 <> 127 Then
                        Write(2, theDungeon(x).dungeonGrid(x1, y1))
                    Else
                        WriteLine(2, theDungeon(x).dungeonGrid(x1, y1))
                    End If
                Next
            Next
            WriteLine(2, theDungeon(x).entranceUsed)
            WriteLine(2, theDungeon(x).storedXpos)
            WriteLine(2, theDungeon(x).storedYpos)
            WriteLine(2, theDungeon(x).trackName)
            WriteLine(2, theDungeon(x).floorName)
            WriteLine(2, theDungeon(x).floorFGColour)
            WriteLine(2, theDungeon(x).floorBGColour)
            WriteLine(2, theDungeon(x).NPCshop.NPCid)
            Console.Write(".")
        Next
        WriteLine(2, gristCache)
        WriteLine(2, dungeonXPos)
        WriteLine(2, dungeonYPos)
        WriteLine(2, playerarray(1).playerId)
        WriteLine(2, playerarray(1).firstName)
        WriteLine(2, playerarray(1).lastName)
        WriteLine(2, playerarray(1).chumhandle)
        WriteLine(2, playerarray(1).chumhandleAbrv)
        WriteLine(2, playerarray(1).ageYears)
        WriteLine(2, playerarray(1).ageSweeps)
        WriteLine(2, playerarray(1).gender)
        WriteLine(2, playerarray(1).lunarSway)
        WriteLine(2, playerarray(1).classTitle)
        WriteLine(2, playerarray(1).aspectTitle)
        WriteLine(2, playerarray(1).species)
        For x = 0 To 7
            WriteLine(2, playerarray(1).typingQuirk(x))
        Next
        WriteLine(2, playerarray(1).eyeColour)
        For x = 0 To 11
            WriteLine(2, playerarray(1).stats(x))
        Next
        WriteLine(2, playerarray(1).initialMaxHealth)
        WriteLine(2, playerarray(1).alive)
        WriteLine(2, playerarray(1).initialMaxQu)
        WriteLine(2, playerarray(1).initialHandSize)
        WriteLine(2, playerarray(1).currentHealth)
        For x = 0 To playerarray(1).initialRegistry.Count - 1
            WriteLine(2, playerarray(1).initialRegistry(x))
        Next

        FileClose(2)

        Console.Clear()
        Console.WriteLine("Save Complete!")
        BlankLine()
        Beat()

    End Sub

    Public Sub LoadRun(filedir)
        theDungeon.Clear()
        Dim loadPlayer As New Player
        Dim loadStr As String

        Console.Clear()
        Console.Write("Loading")
        FileOpen(3, CurDir() & filedir, OpenMode.Input)

        For x = 0 To 5
            Dim loadFloor As New DungeonFloor(x, 1)

            loadFloor.floorNo = LineInput(3)

            Dim mapRow(127) As String
            For i = 0 To 127
                mapRow(i) = LineInput(3)
                mapRow(i) = Replace(mapRow(i), ",", "")
            Next

            For y1 = 0 To 127
                For x1 = 0 To 127
                    loadFloor.dungeonGrid(x1, y1) = mapRow(y1).Substring(x1, 1)
                Next
            Next

            If LineInput(3) = "#FALSE#" Then
                loadFloor.entranceUsed = False
            Else
                loadFloor.entranceUsed = True
            End If

            loadFloor.storedXpos = LineInput(3)
            loadFloor.storedYpos = LineInput(3)
            loadStr = LineInput(3)
            loadFloor.trackName = loadStr.Substring(1, loadStr.Length - 2)
            loadStr = LineInput(3)
            loadFloor.floorName = loadStr.Substring(1, loadStr.Length - 2)
            loadStr = LineInput(3)
            loadFloor.floorFGColour = loadStr.Substring(1, loadStr.Length - 2)
            loadStr = LineInput(3)
            loadFloor.floorBGColour = loadStr.Substring(1, loadStr.Length - 2)
            loadFloor.NPCshop.NPCid = LineInput(3)

            theDungeon.Add(loadFloor)
            Console.Write(".")
        Next

        gristCache = LineInput(3)
        dungeonXPos = LineInput(3)
        dungeonYPos = LineInput(3)
        loadPlayer.playerId = LineInput(3)

        loadStr = LineInput(3)
        loadPlayer.firstName = loadStr.Substring(1, loadStr.Length - 2)
        loadStr = LineInput(3)
        loadPlayer.lastName = loadStr.Substring(1, loadStr.Length - 2)
        loadStr = LineInput(3)
        loadPlayer.chumhandle = loadStr.Substring(1, loadStr.Length - 2)
        loadStr = LineInput(3)
        loadPlayer.chumhandleAbrv = loadStr.Substring(1, loadStr.Length - 2)

        loadPlayer.ageYears = LineInput(3)
        loadPlayer.ageSweeps = LineInput(3)

        loadStr = LineInput(3)
        loadPlayer.gender = loadStr.Substring(1, loadStr.Length - 2)

        loadPlayer.lunarSway = LineInput(3)
        loadPlayer.classTitle = LineInput(3)
        loadPlayer.aspectTitle = LineInput(3)
        loadPlayer.species = LineInput(3)
        For x = 0 To 7
            loadPlayer.typingQuirk(x) = LineInput(3)
        Next

        loadStr = LineInput(3)
        loadPlayer.eyeColour = loadStr.Substring(1, loadStr.Length - 2)

        For x = 0 To 11
            loadPlayer.stats(x) = LineInput(3)
        Next
        loadPlayer.initialMaxHealth = LineInput(3)

        If LineInput(3) = "#FALSE#" Then
            loadPlayer.alive = False
        Else
            loadPlayer.alive = True
        End If

        loadPlayer.initialMaxQu = LineInput(3)
        loadPlayer.initialHandSize = LineInput(3)
        loadPlayer.currentHealth = LineInput(3)

        Do Until EOF(3)
            loadPlayer.initialRegistry.Add(LineInput(3))
        Loop

        playerarray(1) = loadPlayer

        FileClose(3)

        Console.Clear()
        Console.WriteLine("Load Complete")
        BlankLine()
        Beat()
    End Sub

    'General Usage Subs/Functions
    Public Sub FillPlayerArray() 'fills up the player arrays so that visual basic stops crying
        For x = 1 To MaxPlayerCount 'generating the players
            Dim randomlygeneratedhuman As New Player
            randomlygeneratedhuman.HumanGen()
            playerarray(x) = randomlygeneratedhuman
        Next
    End Sub

    Public Sub ResetPlayerArray()
        For x = 1 To MaxPlayerCount
            playerarray(x).ClearSelf()
        Next
    End Sub

    Public Function AnyEnemiesAlive()
        Dim areAlive = False
        For x = 0 To enemyList.Count - 1
            If enemyList(x).alive Then
                areAlive = True
            End If
        Next
        Return areAlive
    End Function

    Public Function SoundTest(input As Integer)
        Console.Clear()
        Console.WriteLine("Current Track: " + ReadTextFile("\data\tracknames.txt", input + 1))
        BlankLine()
        Console.WriteLine("A & D - Select Track")
        Console.WriteLine("0 - Back")
        BlankLine()
        PlayMusic(ReadTextFile("\data\trackinternalnames.txt", input + 1), True)
        Return Console.ReadKey(True).KeyChar
    End Function

    Public Sub PlayMusic(track As String, isLoop As Boolean)
        If track <> currentlyPlayingTrack And isMusicMuted = False Then
            currentlyPlayingTrack = track
            My.Computer.Audio.Stop()
            If isLoop = True Then
                My.Computer.Audio.Play(CurDir() & "\music\" & track & ".wav", AudioPlayMode.BackgroundLoop)
            Else
                My.Computer.Audio.Play(CurDir() & "\music\" & track & ".wav", AudioPlayMode.Background)
            End If
        End If
    End Sub

    Public Function ConvertYearsToSweeps(i As Double)
        Return i * (6 / 13)
    End Function

    Public Function ConvertSweepsToYears(i As Double)
        Return i * (13 / 6)
    End Function

    Public Sub ReadAndOutputTextFile(filedir As String, lineToRead As Integer)
        Dim currentLine As String = ""
        Dim counter As Integer = 0
        Dim lineExistsCheck As Boolean = False
        FileOpen(1, CurDir() & filedir, OpenMode.Input)
        Do Until EOF(1)
            currentLine = LineInput(1)
            counter += 1
            If lineToRead = 0 Then
                Console.WriteLine(currentLine)
            ElseIf lineToRead = counter Then
                Console.WriteLine(currentLine)
                lineExistsCheck = True
            End If
        Loop

        If lineToRead = 0 Then
            lineExistsCheck = True
        End If

        If lineExistsCheck = False Then
            ErrorCustomMsg("Line doesn't exist in called file.")
        End If

        FileClose(1)
    End Sub 'REPLACE ALL USES OF THIS WITH READTEXTFILE()

    Public Function ReadTextFile(filedir As String, lineToRead As Integer)
        Dim currentLine As String = ""
        Dim returnline As String = ""
        Dim counter As Integer = 0
        Dim lineExistsCheck As Boolean = False
        FileOpen(1, CurDir() & filedir, OpenMode.Input)
        Do Until EOF(1) Or lineExistsCheck = True
            currentLine = LineInput(1)
            counter += 1
            If lineToRead = counter Then
                returnline = currentLine
                lineExistsCheck = True
            End If
        Loop

        If lineExistsCheck = False Then
            ErrorCustomMsg("Line doesn't exist in called file.")
        End If

        FileClose(1)

        Return returnline
    End Function

    Public Function getCleanNumericalInput()
        Dim strInput As String
        Dim intInput As Integer

        strInput = ReturnNumbersOnly(Console.ReadLine())
        If strInput <> "" And strInput.Length < 10 Then 'ADD: Make this not cause a crash when non-integer, non-null values are input
            intInput = strInput
        Else
            intInput = 0
        End If

        Return intInput
    End Function

    'Here onwards lie the subroutines that are the spawn of a far less experienced coder,
    'who wasted a lot of time coding this drivel instead of doing it the far easier,
    'if a bit more mentally taxing, way.
    '(what i'm saying is a lot of this code past here sucks, and i hope to get to replacing them with better subroutines, or entirely removing the need for them in the first place)
    '(probably during the testing period)

    Public Function StatName(i As Integer)
        Select Case i
            Case 1
                Return "Vim"
            Case 2
                Return "Alacrity"
            Case 3
                Return "Vivification"
            Case 4
                Return "Surreptitiousness"
            Case 5
                Return "Inviolability"
            Case 6
                Return "Adroitness"
            Case 7
                Return "Pulchritude"
            Case 8
                Return "Imagination"
            Case 9
                Return "Gumption"
            Case 10
                Return "Pluck"
            Case 11
                Return "Propitiousness"
            Case Else
                ErrorMsg(2)
        End Select
    End Function

    Public Function StatNameAbrv(i As Integer)
        Select Case i
            Case 1
                Return "VIM"
            Case 2
                Return "ACR"
            Case 3
                Return "VIV"
            Case 4
                Return "SRP"
            Case 5
                Return "INV"
            Case 6
                Return "ADR"
            Case 7
                Return "PCH"
            Case 8
                Return "IMG"
            Case 9
                Return "GMP"
            Case 10
                Return "PLK"
            Case 11
                Return "PRP"
            Case Else
                ErrorMsg(2)
        End Select
    End Function

    Public Function LunarSwayIntToString(i As Integer)
        Select Case i
            Case 1
                Return "Prospit"
            Case 2
                Return "Derse"
            Case Else
                ErrorMsg(1)
        End Select
    End Function

    Public Function AspectIntToString(i As Integer)
        Select Case i
            Case 1
                Return "Time"
            Case 2
                Return "Space"
            Case 3
                Return "Heart"
            Case 4
                Return "Mind"
            Case 5
                Return "Hope"
            Case 6
                Return "Rage"
            Case 7
                Return "Light"
            Case 8
                Return "Void"
            Case 9
                Return "Breath"
            Case 10
                Return "Blood"
            Case 11
                Return "Life"
            Case 12
                Return "Doom"
            Case Else
                ErrorMsg(3)
        End Select
    End Function

    Public Function ClassIntToString(i As Integer)
        Select Case i
            Case 1
                Return "Heir"
            Case 2
                Return "Witch"
            Case 3
                Return "Page"
            Case 4
                Return "Knight"
            Case 5
                Return "Seer"
            Case 6
                Return "Mage"
            Case 7
                Return "Sylph"
            Case 8
                Return "Maid"
            Case 9
                Return "Thief"
            Case 10
                Return "Rogue"
            Case 11
                Return "Prince"
            Case 12
                Return "Bard"
                'dead session classes
            Case 13
                Return "Lord"
            Case 14
                Return "Muse"
            Case Else
                ErrorMsg(4)
        End Select
    End Function

    Public Function BloodCasteIntToString(i As Integer)
        Select Case i
            Case 1
                Return "Rust"
            Case 2
                Return "Bronze"
            Case 3
                Return "Gold"
            Case 4
                Return "Lime"
            Case 5
                Return "Olive"
            Case 6
                Return "Jade"
            Case 7
                Return "Teal"
            Case 8
                Return "Blue"
            Case 9
                Return "Indigo"
            Case 10
                Return "Purple"
            Case 11
                Return "Violet"
            Case 12
                Return "Fuchsia"
            Case Else
                ErrorMsg(5)
        End Select
    End Function

    Public Function SpeciesIntToString(i As Integer)
        Select Case i
            Case 0
                Return "Human"
            Case 1
                Return "Troll"
            Case Else
                ErrorMsg(6)
        End Select
    End Function

    Public Function KindAbstratusIntToString(i As Integer, state As Integer)
        Select Case i
            Case 1
                Return "snprrflekind"
            Case 2
                Return "rcktlnchkind"
            Case 3
                If state = 0 Then
                    Return "bladekind"
                Else
                    Return "1/2bladekind"
                End If
            Case 4
                If state = 0 Then
                    Return "hammerkind"
                Else
                    Return "handlekind"
                End If
            Case 5
                If state = 0 Then
                    Return "shotgunkind"
                Else
                    Return "sawedoffkind"
                End If
            Case 6
                Return "batkind"
            Case 7
                Return "canekind"
            Case 8
                Return "dicekind"
            Case 9
                Return "sicklekind"
            Case 10
                Return "clawkind"
            Case 11
                Return "chainsawkind"
            Case 12
                Return "bowkind"
        End Select
    End Function

    Public Function ExtendedZodiacSign(bloodcaste As Integer, lunarsway As Integer, aspect As Integer)
        Dim sign(12, 2, 12) As String 'Blood Caste, Lunar Sway, Aspect
        Dim file As String = "\data\extendedzodiacsigns.txt"
        Dim returnsign As String
        FileOpen(1, CurDir() & file, OpenMode.Input)
        For x = 1 To 12
            For y = 1 To 2
                For z = 1 To 12
                    Dim i As String = LineInput(1)
                    sign(x, y, z) = i
                    If x = bloodcaste And y = lunarsway And z = aspect Then
                        returnsign = i
                    End If
                    'Console.WriteLine(BloodCasteIntToString(x) & "/" & LunarSwayIntToString(y) & "/" & AspectIntToString(z) & ": " & i)
                Next
            Next
        Next
        FileClose(1)
        Return returnsign
    End Function

    Public Sub SetTextToEyeColour(i As Integer)
        Select Case i
            Case 1
                Console.ForegroundColor = ConsoleColor.DarkBlue
            Case 2
                Console.ForegroundColor = ConsoleColor.DarkGreen
            Case 3
                Console.ForegroundColor = ConsoleColor.DarkCyan
            Case 4
                Console.ForegroundColor = ConsoleColor.DarkRed
            Case 5
                Console.ForegroundColor = ConsoleColor.DarkMagenta
            Case 6
                Console.ForegroundColor = ConsoleColor.DarkYellow
            Case 7
                Console.ForegroundColor = ConsoleColor.Blue
            Case 8
                Console.ForegroundColor = ConsoleColor.Green
            Case 9
                Console.ForegroundColor = ConsoleColor.Cyan
            Case 10
                Console.ForegroundColor = ConsoleColor.Red
            Case 11
                Console.ForegroundColor = ConsoleColor.Magenta
            Case 12
                Console.ForegroundColor = ConsoleColor.Yellow
        End Select
    End Sub

    Public Sub SetTextToBloodColour(i As Integer)
        Select Case i
            Case 1
                Console.ForegroundColor = ConsoleColor.DarkRed
            Case 2
                Console.ForegroundColor = ConsoleColor.DarkYellow
            Case 3
                Console.ForegroundColor = ConsoleColor.Yellow
            Case 4
                Console.ForegroundColor = ConsoleColor.Green
            Case 5
                Console.ForegroundColor = ConsoleColor.DarkGreen
            Case 6
                Console.ForegroundColor = ConsoleColor.Green
            Case 7
                Console.ForegroundColor = ConsoleColor.Cyan
            Case 8
                Console.ForegroundColor = ConsoleColor.DarkCyan
            Case 9
                Console.ForegroundColor = ConsoleColor.Blue
            Case 10
                Console.ForegroundColor = ConsoleColor.DarkBlue
            Case 11
                Console.ForegroundColor = ConsoleColor.DarkMagenta
            Case 12
                Console.ForegroundColor = ConsoleColor.Magenta
                'mutant bloods
            Case 13
                Console.ForegroundColor = ConsoleColor.Gray
            Case 14
                Console.ForegroundColor = ConsoleColor.Red
        End Select
    End Sub

    Public Sub ResetTextColour()
        Console.ForegroundColor = ConsoleColor.White
    End Sub

    Public Sub BlankLine()
        Console.WriteLine("")
    End Sub

    Public Sub Beat()
        Threading.Thread.Sleep(1000)
    End Sub

    Public Function ReturnNumbersOnly(sString As String) As String
        Dim i As Integer
        Dim returnstring As String = ""

        If Mid(sString, 1, 1) Like "-" Then
            returnstring = "-"
        End If

        For i = 1 To Len(sString)
            If Mid(sString, i, 1) Like "[0-9]" Then
                returnstring = returnstring + Mid(sString, i, 1)
            End If
        Next
        Return returnstring
    End Function

    Function LowercaseFirstLetter(ByVal val As String) As String 'edited version of https://www.dotnetperls.com/uppercase-first-letter-vbnet
        ' Test for nothing or empty.
        If String.IsNullOrEmpty(val) Then
            Return val
        End If

        ' Convert to character array.
        Dim array() As Char = val.ToCharArray

        ' Uppercase first character.
        array(0) = Char.ToLower(array(0))

        ' Return new string.
        Return New String(array)
    End Function

    Function UppercaseFirstLetter(ByVal val As String) As String 'copied from https://www.dotnetperls.com/uppercase-first-letter-vbnet
        ' Test for nothing or empty.
        If String.IsNullOrEmpty(val) Then
            Return val
        End If

        ' Convert to character array.
        Dim array() As Char = val.ToCharArray

        ' Uppercase first character.
        array(0) = Char.ToUpper(array(0))

        ' Return new string.
        Return New String(array)
    End Function

    Function AlternatingCaps(ByVal val As String) As String 'edited version of https://www.dotnetperls.com/uppercase-first-letter-vbnet
        ' Test for nothing or empty.
        If String.IsNullOrEmpty(val) Then
            Return val
        End If

        ' Convert to character array.
        Dim array() As Char = val.ToCharArray

        'Find array length
        Dim altlength As Integer = UBound(array) - LBound(array) + 1
        Dim altcount As Integer = 0

        ' uppercase every other letter
        If altlength > 1 Then
            Do
                array(altcount) = Char.ToUpper(array(altcount))
                If altcount + 1 < altlength Then
                    array(altcount + 1) = Char.ToLower(array(altcount + 1))
                End If
                altcount += 2
            Loop Until altcount + 1 > altlength
        End If

        ' Return new string.
        Return New String(array)
    End Function

    Public Sub ErrorMsg(msg As Integer)
        Dim i As String = Console.ForegroundColor
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("*****************************************************************")
        Console.WriteLine("ERROR: " & msg.ToString)
        Console.WriteLine("Press ENTER to continue.")
        Console.WriteLine("*****************************************************************")
        Console.ReadLine()
        Console.ForegroundColor = i
    End Sub

    Public Sub ErrorCustomMsg(msg As String)
        Dim i As String = Console.ForegroundColor
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("*****************************************************************")
        Console.WriteLine("ERROR: " & msg)
        Console.WriteLine("Press ENTER to continue.")
        Console.WriteLine("*****************************************************************")
        Console.ReadLine()
        Console.ForegroundColor = i
    End Sub

End Module