I=0
FILES=$(find "./Pieces/" -type f)

for FILE in $FILES; do
	echo $FILE
	mv $FILE "./Pieces/piece"$I".png"
	I=$((I+1))
done
