public class Soundex { 
    public static String soundex(String s) { 
        char[] x = s.toUpperCase().toCharArray();
        char firstLetter = x[0];

        // convert letters to numeric code
        for (int i = 0; i < x.length; i++) {
            switch (x[i]) {
                case 'B':
                case 'F':
                case 'P':
                case 'V': { x[i] = '1'; break; }

                case 'C':
                case 'G':
                case 'J':
                case 'K':
                case 'Q':
                case 'S':
                case 'X':
                case 'Z': { x[i] = '2'; break; }

                case 'D':
                case 'T': { x[i] = '3'; break; }

                case 'L': { x[i] = '4'; break; }

                case 'M':
                case 'N': { x[i] = '5'; break; }

                case 'R': { x[i] = '6'; break; }

                default:  { x[i] = '0'; break; }
            }
        }

        // remove duplicates
        String output = "" + firstLetter;
        for (int i = 1; i < x.length; i++)
            if (x[i] != x[i-1] && x[i] != '0')
                output += x[i];

        // pad with 0's or truncate
        output = output + "0000";
        return output.substring(0, 4);
    }


    public static void main(String[] args) {
        String name1 = "levi";
        String name2 = "levy";
        String name3 = "levine";
        String name4 = "loweb";
        String name5 = "lovi";
        
        String name6 = "cohen";
        String name7 = "choen";
        String name8 = "kohen";
        String name9 = "kahana";
        String name10 = "khan"; 
        String name11 = "cownn";
      
        String name12 = "geringen";
        String name13 = "grown";
        String name14 = "grune";
        String name15 = "grin";
        String name16 = "green";
        
        String name17 = "tamir";
        String name18 = "tamar";
        String name19 = "tmeer";
        String name20 = "tomer";
        
        
        
        System.out.println(soundex(name1) + ": " + name1);
        System.out.println(soundex(name2) + ": " + name2);
        System.out.println(soundex(name3) + ": " + name3);
        System.out.println(soundex(name4) + ": " + name4);
        System.out.println(soundex(name5) + ": " + name5);
        System.out.println("---------------------------------");
        System.out.println(soundex(name6) + ": " + name6);
        System.out.println(soundex(name7) + ": " + name7);
        System.out.println(soundex(name8) + ": " + name8);
        System.out.println(soundex(name9) + ": " + name9);
        System.out.println(soundex(name10) + ": " + name10);
        System.out.println(soundex(name11) + ": " + name11);
        System.out.println("----------------------------------");
        System.out.println(soundex(name12) + ": " + name12);
        System.out.println(soundex(name13) + ": " + name13);
        System.out.println(soundex(name14) + ": " + name14);
        System.out.println(soundex(name15) + ": " + name15);
        System.out.println(soundex(name16) + ": " + name16);
        System.out.println("----------------------------------");
    
        System.out.println(soundex(name17) + ": " + name17);
        System.out.println(soundex(name18) + ": " + name18);
        System.out.println(soundex(name19) + ": " + name19);
        System.out.println(soundex(name20) + ": " + name20);
        System.out.println("----------------------------------");
    
    
    
    }
}

