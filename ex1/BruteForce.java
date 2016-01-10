import java.util.Scanner;


public class BruteForce {

	
	public static class BruteForceSearch {

		private char[] text;
		private char[] pattern;
		private int n;
		private int m;
		
		public void setString(String t, String p) {
			this.text = t.toCharArray();
			this.pattern = p.toCharArray();
			this.n = t.length();
			this.m = p.length();
		}
		
		public int search() {
			for (int i = n-m; i > 0; i--) {
				int j = 0;
				while (j < m && text[i+j] == pattern[j]) {
					j++;
				}
				if (j == m) return i;
			}
			return -1;
		}
	}
	
	public static void main(String[] args) {
		Scanner a = new Scanner(System.in);
		BruteForceSearch bfs = new BruteForceSearch();
		
		System.out.println("שמי תמיר אבו סלאח");
		String text = "שמי תמיר אבו סלאח";
		System.out.println("תמיר");
		String pattern = "תמיר";
		bfs.setString(text, pattern);
		System.out.println(bfs.search());
		
		System.out.println("היי מה קורה");
		text = "היי מה קורה";
		System.out.println("היי");
		pattern = "היי";
		bfs.setString(text, pattern);
		System.out.println(bfs.search());
		
		
	}


}
