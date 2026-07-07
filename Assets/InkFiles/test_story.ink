VAR gold = 100

=== start ===
В кабарэ входит комиссар. У вас есть {gold} золотых. Он намекает на взятку. #speaker:Policeman
+ [Дать взятку (50 gold)]
    ~ gold = gold - 50
    Ладно, я ничего не видел. #speaker:Policeman
    -> ending1
+ [Fuck you]
    Ходи и бойся, чучело. #speaker:Policeman
    -> ending2

=== ending1 ===
(В кошельке осталось {gold} монеток и вы в безопасности)
-> END

=== ending2 ===
(В кошельке осталось {gold} монеток, но вам пизда)
-> END
