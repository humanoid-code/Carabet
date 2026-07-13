// === АДЕЛАИДА ГРИТЦ ===

// --- ДИАЛОГ 1 ---
=== adelaide_dialogue_1 ===
# char:AdelaideGritz
Аделаида Гритц: Диалог 1. Шаг 1.
* [Выбор 1.1] 
# money:+10 
-> adelaide_d1_s2_A
* [Выбор 1.2] 
# law:-10 
-> adelaide_d1_s2_B

=== adelaide_d1_s2_A ===
Аделаида Гритц: Диалог 1. Шаг 2 (Ветка А).
* [Выбор 1.3] 
# rating:+5 
-> adelaide_d1_s3_A
* [Выбор 1.4] 
# workers_wellbeing:-5 
-> adelaide_d1_s3_B

=== adelaide_d1_s2_B ===
Аделаида Гритц: Диалог 1. Шаг 2 (Ветка Б).
* [Выбор 1.5] # money:-15 
-> adelaide_d1_s3_C
* [Выбор 1.6] # law:+5 
-> adelaide_d1_s3_D

=== adelaide_d1_s3_A ===
Аделаида Гритц: Диалог 1. Шаг 3 (Финал А).
* [ДАЛЕЕ] -> END
=== adelaide_d1_s3_B ===
Аделаида Гритц: Диалог 1. Шаг 3 (Финал Б).
* [ДАЛЕЕ] -> END
=== adelaide_d1_s3_C ===
Аделаида Гритц: Диалог 1. Шаг 3 (Финал В).
* [ДАЛЕЕ] -> END
=== adelaide_d1_s3_D ===
Аделаида Гритц: Диалог 1. Шаг 3 (Финал Г).
* [ДАЛЕЕ] -> END


// --- ДИАЛОГ 2 ---
=== adelaide_dialogue_2 ===
# char:AdelaideGritz
Аделаида Гритц: Диалог 2. Шаг 1.
* [Выбор 2.1] # rating:+10 
-> adelaide_d2_s2_A
* [Выбор 2.2] # money:-20 
-> adelaide_d2_s2_B

=== adelaide_d2_s2_A ===
Аделаида Гритц: Диалог 2. Шаг 2 (Ветка А).
* [Выбор 2.3] # law:+10 
-> adelaide_d2_s3_A
* [Выбор 2.4] # workers_wellbeing:-10 
-> adelaide_d2_s3_B

=== adelaide_d2_s2_B ===
Аделаида Гритц: Диалог 2. Шаг 2 (Ветка Б).
* [Выбор 2.5] # money:+25 
-> adelaide_d2_s3_C
* [Выбор 2.6] # rating:-5 
-> adelaide_d2_s3_D

=== adelaide_d2_s3_A ===
Аделаида Гритц: Диалог 2. Шаг 3 (Финал А).
* [ДАЛЕЕ] -> END
=== adelaide_d2_s3_B ===
Аделаида Гритц: Диалог 2. Шаг 3 (Финал Б).
* [ДАЛЕЕ] -> END
=== adelaide_d2_s3_C ===
Аделаида Гритц: Диалог 2. Шаг 3 (Финал В).
* [ДАЛЕЕ] -> END
=== adelaide_d2_s3_D ===
Аделаида Гритц: Диалог 2. Шаг 3 (Финал Г).
* [ДАЛЕЕ] -> END


// --- ДИАЛОГ 3 ---
=== adelaide_dialogue_3 ===
# char:AdelaideGritz
Аделаида Гритц: Диалог 3. Шаг 1.
* [Выбор 3.1] # workers_wellbeing:+15 
-> adelaide_d3_s2_A
* [Выбор 3.2] # money:+30 
-> adelaide_d3_s2_B

=== adelaide_d3_s2_A ===
Аделаида Гритц: Диалог 3. Шаг 2 (Ветка А).
* [Выбор 3.3] # law:-15 
-> adelaide_d3_s3_A
* [Выбор 3.4] # rating:-5 
-> adelaide_d3_s3_B

=== adelaide_d3_s2_B ===
Аделаида Гритц: Диалог 3. Шаг 2 (Ветка Б).
* [Выбор 3.5] # money:-20 
-> adelaide_d3_s3_C
* [Выбор 3.6] # workers_wellbeing:+5 
-> adelaide_d3_s3_D

=== adelaide_d3_s3_A ===
Аделаида Гритц: Диалог 3. Шаг 3 (Финал А).
* [ДАЛЕЕ] -> END
=== adelaide_d3_s3_B ===
Аделаида Гритц: Диалог 3. Шаг 3 (Финал Б).
* [ДАЛЕЕ] -> END
=== adelaide_d3_s3_C ===
Аделаида Гритц: Диалог 3. Шаг 3 (Финал В).
* [ДАЛЕЕ] -> END
=== adelaide_d3_s3_D ===
Аделаида Гритц: Диалог 3. Шаг 3 (Финал Г).
* [ДАЛЕЕ] -> END