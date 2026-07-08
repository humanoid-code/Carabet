VAR gold = 100
VAR reputation = 50
VAR danger = 0

=== start ===
В кабарэ входит комиссар. У вас есть {gold} золотых, репутация {reputation}, опасность {danger}/100. Офицер намекает на взятку. #speaker:Policeman
+ [Дать взятку (50 gold)]
    ~ gold = gold - 50
    Ладно, я ничего не видел. #speaker:Policeman
    -> ending1
+ [Fuck you]
    ~ danger = danger + 40
    Ходи и бойся, чучело. #speaker:Policeman
    -> ending2
+ [Попытаться заговорить]
    Бла-бла-бла #speaker:Policeman
    -> ending3
+ [Обосраться]
    ~ reputation = reputation - 25
    Блять... #speaker:Policeman
    -> ending4
    
=== ending1 ===
(В кошельке осталось {gold} монеток и вы в безопасности, репутация {reputation}, опасность {danger})
-> END

=== ending2 ===
(В кошельке осталось {gold} монеток, но вам пизда: опасность {danger})
-> END

=== ending3 ===
(бла-бла-бла и тут дальше диалог)
-> END

=== ending4 ===
(Вы обосрались! В кошельке осталось {gold} монеток и вы в безопасности, но репутация {reputation}, опасность {danger})
-> END
