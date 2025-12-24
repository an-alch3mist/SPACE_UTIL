is there name for these type of prompt ? if so let me know ?

also btw regarding loop timing 
```py
sum = 0
# should be instant since just 100 iter
for i in range(100):
	sum += 1
print(sum)
```

```py
sum = 0
# should be ~ 10 frame timings Time.delta Time or 10 x yield return null since just 100 x 10iter, for every 100 iter it takes calculates in one go with just 1 frame delay
for i in range(1000):
	sum += 1
print(sum)
```

```py
sum = 0
#  takes calculates in 1 go
for y in range(50): # instant since its < 100 frames
	for x in range(1000): # takes 10 x frames or yield return null
		sum += 1
print(sum)
```

```py
sum = 0
#  takes calculates in 1 go
for i in range(50): # instant since its < 100 frames, just 1 frame delay overall
	print("somthng")
	if i % 10 == 0:
		sleep(2) # shall wait for exact 2 sec yield return new wait for second(2), btw a number can be decimal or non decimal should work as inteded store them as long or double or everythong in double as you seem fit
print(sum)
```

also what about grouping logics, such as if (1 == 0) and 2 < 3 or 5 > 2: should work where ever condition required as in traditional python. 

make these changes and include more test suites inside the prompt along with preexisting prompt, test(check) suite and more, so that all edge cases are covered without leaving any and provide the final prompt in a mark down file(~1500 - 2000 lines)