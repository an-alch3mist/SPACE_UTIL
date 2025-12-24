namespace LOOPLanguage
{
    /// <summary>
    /// Collection of demo scripts for testing the interpreter
    /// Based on the test suite from the specification
    /// </summary>
    public static class DemoScripts
    {
        #region Advanced Lambda Tests
        
        public const string LAMBDA_WITH_LIST_COMP = @"
nums = [1, 2, 3, 4, 5, 6, 7, 8]
result = (lambda x: [i*i for i in x if i % 2 == 0 and i > 3])(nums)
print(result)
";
        
        public const string SORTED_WITH_LAMBDA_TUPLE = @"
data = [(1, 'b'), (3, 'a'), (2, 'c')]
sorted_data = sorted(data, key=lambda x: x[1])
print(sorted_data)
";
        
        public const string LAMBDA_MULTIPLE_CONDITIONS = @"
nums = [-5, 2, 15, 50, 101, 88]
filter_func = lambda x: x > 0 and x % 2 == 0 and x < 100
result = [x for x in nums if filter_func(x)]
print(result)
";
        
        public const string COMPLEX_IIFE = @"
result = (lambda nums: [
    x * 2 for x in nums 
    if x % 2 == 1 and x > 2
])([1, 2, 3, 4, 5, 6, 7, 8, 9])

print(result)
";
        
        public const string LAMBDA_SORT_DICT = @"
items = [
    {'name': 'apple', 'price': 3, 'qty': 10},
    {'name': 'banana', 'price': 1, 'qty': 20},
    {'name': 'cherry', 'price': 2, 'qty': 5}
]

sorted_items = sorted(items, key=lambda x: x['price'] * x['qty'])

for item in sorted_items:
    print(item['name'])
";
        
        public const string LAMBDA_NESTED_INDEXING = @"
matrix = [[1, 2, 3], [4, 5, 6], [7, 8, 9]]
sorted_matrix = sorted(matrix, key=lambda row: row[1])

for row in sorted_matrix:
    print(row)
";
        
        public const string LAMBDA_MULTI_PARAM = @"
combine = lambda a, b, c: a + b * c
result = combine(10, 5, 2)
print(result)

make_pair = lambda x, y: (x, y)
pair = make_pair(5, 10)
print(pair)
";
        
        public const string LAMBDA_DESCENDING_SORT = @"
data = [(1, 'b'), (3, 'a'), (2, 'c')]
sorted_data = sorted(data, key=lambda x: x[0], reverse=True)
print(sorted_data)
";
        
        #endregion
        
        #region Tuple Tests
        
        public const string TUPLE_BASIC = @"
t = (1, 'a', 3.14)
print(t[0])
print(t[1])
print(t[2])
print(t[-1])
";
        
        public const string TUPLE_IN_LIST = @"
data = [(1, 2), (3, 4), (5, 6)]
for pair in data:
    print(pair[0] + pair[1])
";
        
        public const string TUPLE_SINGLE_ELEMENT = @"
t = (42,)
print(len(t))
print(t[0])
";
        
        #endregion
        
        #region Enum Tests
        
        public const string ENUM_BASIC = @"
if get_ground_type() == Grounds.Soil:
    print('Standing on soil')

ground = get_ground_type()
if ground == Grounds.Grassland:
    till()
elif ground == Grounds.Soil:
    plant(Entities.Carrot)
";
        
        public const string ENUM_FUNCTION_CALLS = @"
plant(Entities.Carrot)
if can_harvest():
    harvest()

if num_items(Items.Carrot) > 0:
    print('Have carrots!')
";
        
        #endregion
        
        #region Built-in Constants
        
        public const string CONSTANTS_MOVEMENT = @"
move(North)
move(East)
move(South)
move(West)
";
        
        #endregion
        
        #region Operator Precedence
        
        public const string EXPONENTIATION_PRECEDENCE = @"
result = 2 ** 3 ** 2
print(result)

result2 = 2 * 3 ** 2
print(result2)
";
        
        #endregion
        
        #region List Operations
        
        public const string LIST_NEGATIVE_INDEX = @"
items = [10, 20, 30, 40, 50]
print(items[-1])
print(items[-2])
";
        
        public const string LIST_SLICING = @"
nums = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
print(nums[2:5])
print(nums[:3])
print(nums[7:])
";
        
        public const string LIST_COMPREHENSION = @"
nums = [1, 2, 3, 4, 5]
doubled = [x * 2 for x in nums]
print(doubled)

evens = [x for x in nums if x % 2 == 0]
print(evens)
";
        
        #endregion
        
        #region Instruction Budget Test
        
        public const string INSTRUCTION_BUDGET_TEST = @"
sum = 0
for i in range(100):
    sum = sum + 1
print(sum)
";
        
        public const string LARGE_LOOP_TEST = @"
sum = 0
for i in range(1000):
    sum = sum + 1
print(sum)
";
        
        #endregion
        
        #region Game Functions Test
        
        public const string GAME_FUNCTIONS_BASIC = @"
for i in range(5):
    move(North)
    if can_harvest():
        harvest()

plant(Entities.Carrot)
";
        
        public const string GAME_FUNCTIONS_GRID = @"
for x in range(get_world_size()):
    for y in range(get_world_size()):
        if get_ground_type() == Grounds.Soil:
            plant(Entities.Grass)
        move(East)
    move(South)
";
        
        #endregion
        
        #region Complex Integration Tests
        
        public const string COMPLEX_INTEGRATION_1 = @"
// Lambda with sorted and list operations
numbers = [5, 2, 8, 1, 9, 3, 7, 4, 6]
sorted_nums = sorted(numbers, key=lambda x: -x)

print('Sorted descending:')
for num in sorted_nums:
    print(num)

// List comprehension with lambda filter
filter_fn = lambda n: n % 2 == 0 and n > 3
filtered = [x for x in numbers if filter_fn(x)]
print('Filtered:', filtered)
";
        
        public const string COMPLEX_INTEGRATION_2 = @"
// Nested data structures with lambda
students = [
    {'name': 'Alice', 'grade': 85},
    {'name': 'Bob', 'grade': 92},
    {'name': 'Charlie', 'grade': 78}
]

// Sort by grade descending
sorted_students = sorted(students, key=lambda s: -s['grade'])

print('Students by grade:')
for student in sorted_students:
    print(student['name'], ':', student['grade'])
";
        
        public const string COMPLEX_INTEGRATION_3 = @"
// Function with lambda and list comprehension
def process_data(data):
    // Filter positive even numbers
    evens = [x for x in data if x > 0 and x % 2 == 0]
    
    // Sort by value
    sorted_evens = sorted(evens, key=lambda x: x)
    
    // Double each value
    doubled = [x * 2 for x in sorted_evens]
    
    return doubled

numbers = [-4, 2, -1, 6, 3, 8, -2, 4]
result = process_data(numbers)
print('Result:', result)
";
        
        #endregion
        
        #region Full Game Scenario
        
        public const string FULL_GAME_SCENARIO = @"
// Complete farming scenario
def harvest_grid():
    world_size = get_world_size()
    
    for y in range(world_size):
        for x in range(world_size):
            // Check ground type
            if get_ground_type() == Grounds.Soil:
                if can_harvest():
                    harvest()
                else:
                    plant(Entities.Carrot)
            elif get_ground_type() == Grounds.Grassland:
                till()
            
            // Move to next tile
            if x < world_size - 1:
                move(East)
        
        // Move to next row
        if y < world_size - 1:
            move(South)
            // Return to start of row
            for i in range(world_size - 1):
                move(West)

// Execute
harvest_grid()
print('Farming complete!')
";
        
        #endregion
    }
}
