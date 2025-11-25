cc = gcc

CFLAGS = -std=c99 -Wall -Wextra -I/opt/homebrew/include
LDFLAGS = -L/opt/homebrew/lib -lraylib -framework Cocoa -framework IOKit -framework CoreVideo

SRC = main.c
OUT = codootoor 

$(OUT): $(SRC)
	$(CC) $(SRC) $(CFLAGS) $(LDFLAGS) -o $(OUT)

clean:
	rm -f $(OUT)
