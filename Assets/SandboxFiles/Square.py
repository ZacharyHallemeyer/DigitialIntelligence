def printSquare(sideLength):
	
	for count in range(sideLength):
		
		print("*" * sideLength)
		
	
def printTriangle(sideLength):
	
	for count in range(sideLength):
	
		if(count % 2 != 0):
			print((" " *((sideLength - count)//2)) + "*" (count))
			

def printTitle(title):
	print("\n\n" + title + "\n\n")
	
	
	
printTitle("SQUARE")
printTitle("SQUARE")
printTitle("SQUARE")
printSquare(5)
printTitle("TRIANGLE")
printTriangle(10)