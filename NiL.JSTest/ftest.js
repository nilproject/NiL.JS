top:
for(i=0; i<10; i++) {
    for(j=0; j<15; j++) {
        if (i==5 && j==5) break top;
    }
}
console.log(i + j);
